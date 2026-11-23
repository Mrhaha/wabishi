using System;
using System.Collections.Generic;

public sealed partial class DiceKingDemo
{
    private sealed class RoundScores
    {
        public readonly bool[] Robbed;
        public readonly int[] ShellCounts;
        public readonly int[] ShellBonuses;
        public readonly int[] LockedPips;
        public readonly int Passes;

        public RoundScores(int size, int passes)
        {
            Robbed = new bool[size];
            ShellCounts = new int[size];
            ShellBonuses = new int[size];
            LockedPips = new int[size];
            Passes = passes;
        }
    }

    private static int SaturatingRoundInt(long value)
    {
        return (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, value));
    }

    private static int RoundScoreWithMultiplier(long sum, float multiplier)
    {
        double result = Math.Round(sum * (double)multiplier, MidpointRounding.ToEven);
        return (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, result));
    }

    private static void AddFaceValues(Die target, int[] sourceFaces)
    {
        if (target == null || target.Faces == null || sourceFaces == null || target.Faces.Length != sourceFaces.Length)
        {
            return;
        }

        for (int i = 0; i < target.Faces.Length; i++)
        {
            target.Faces[i] = SaturatingRoundInt((long)target.Faces[i] + sourceFaces[i]);
        }
    }

    private void ApplyRoundStartEffects()
    {
        int passes = HasDiceOfType(DieType.Rewind) ? 2 : 1;
        for (int pass = 0; pass < passes; pass++)
        {
            foreach (Die host in dice)
            {
                if (host.Temporary || host.TurtleAttachments == null) continue;
                foreach (TurtleAttachment shell in host.TurtleAttachments)
                {
                    if (shell == null) continue;
                    switch (shell.Type)
                    {
                        case DieType.MoneyTurtle:
                            AddLog("金钱龟为" + DieDisplayName(host) + "挣了 1 金。" + AddGoldIncome(1, "金钱龟", true));
                            break;
                        case DieType.TinyTurtle:
                            AddValueToAllFaces(host, 1);
                            AddLog("小小龟使" + DieDisplayName(host) + "六面各 +1。");
                            break;
                        case DieType.DoubleTurtle:
                            int maximum = HighestFaceValue(host);
                            for (int i = 0; i < host.Faces.Length; i++)
                            {
                                if (host.Faces[i] == maximum) host.Faces[i] = SaturatingRoundInt((long)maximum * 2);
                            }
                            AddLog("倍数龟使" + DieDisplayName(host) + "的最大面翻倍。");
                            break;
                    }
                }
            }
        }
    }

    private void ApplyCrownRollResults()
    {
        foreach (Die crown in dice)
        {
            if (crown.Temporary || crown.Type != DieType.Crown) continue;
            List<Die> candidates = dice.FindAll(target => !target.Temporary);
            int choices = Math.Min(2, candidates.Count);
            for (int i = 0; i < choices; i++)
            {
                int selected = UnityEngine.Random.Range(0, candidates.Count);
                Die target = candidates[selected];
                candidates.RemoveAt(selected);
                target.EffectiveValue = HighestFaceValue(target);
                target.RoundNote = AppendNote(target.RoundNote, "王冠定为 " + target.EffectiveValue);
                AddLog("王冠将" + DieDisplayName(target) + "本手点数调为 " + target.EffectiveValue + "。");
            }
            MarkTypeTriggered(crown);
        }
    }

    private RoundScores PrepareRoundScores()
    {
        RoundScores state = new RoundScores(dice.Count, HasDiceOfType(DieType.Duet) ? 2 : 1);
        for (int i = 0; i < dice.Count; i++)
        {
            state.LockedPips[i] = dice[i].EffectiveValue;
            state.ShellCounts[i] = RealTurtleAttachmentCount(dice[i]);
        }
        return state;
    }

    // 预览只推进本手的壳数、计分与抢劫标记，实际提交才改写骰袋、钱包与日志。
    private void ResolveRoundSlot(int slot, RoundScores state, bool commit)
    {
        Die die = dice[slot];
        if (die.Temporary) return;
        int initialShells = die.TurtleAttachments == null ? 0 : die.TurtleAttachments.Count;
        for (int pass = 0; pass < state.Passes; pass++)
        {
            if (die.Type == DieType.RobberyPirate)
            {
                RobRoundNeighbors(slot, state, commit);
            }
            else if (commit && die.Type == DieType.PigFarmer)
            {
                ApplyPigFamilySettlementEffect(die, slot, activeFamilySettlementContext.SettledIds, pass > 0);
            }
            else if (commit && die.Type == DieType.PirateKing)
            {
                ApplyF019PirateSettlementEffects(die, slot, activeFamilySettlementContext.SettledIds, pass > 0);
            }

            for (int i = 0; i < initialShells; i++)
            {
                TurtleAttachment shell = die.TurtleAttachments[i];
                if (shell == null) continue;
                if (shell.Type == DieType.LeaderTurtle)
                {
                    GrantOtherHostScores(slot, state, commit);
                }
                else if (shell.Type == DieType.CaravanTurtle)
                {
                    state.ShellCounts[slot]++;
                    if (commit)
                    {
                        die.TurtleAttachments.Add(new TurtleAttachment
                        {
                            Type = DieType.BasicTurtle,
                            FaceGain = 1,
                            SellValue = 1,
                            Source = TurtleAttachmentSource.RealTurtle
                        });
                        AddValueToAllFaces(die, 1);
                        die.RoundNote = AppendNote(die.RoundNote, "领队吸附基础龟");
                        AddLog(DieDisplayName(die) + "吸附一枚基础龟，六面各 +1，卖价 +1。");
                    }
                }
            }
            if (commit) DrainGameplayChainResponseWaves();
        }
    }

    private void GrantOtherHostScores(int sourceSlot, RoundScores state, bool commit)
    {
        for (int i = 0; i < dice.Count; i++)
        {
            int bonus = state.ShellCounts[i];
            if (i == sourceSlot || state.Robbed[i] || bonus == 0) continue;
            state.ShellBonuses[i] = SaturatingRoundInt((long)state.ShellBonuses[i] + bonus);
            if (!commit) continue;

            Die target = dice[i];
            target.RoundNote = AppendNote(target.RoundNote, "首领龟 +" + bonus);
            if (activeFamilySettlementContext.SettledIds.Contains(target.Id))
            {
                target.Score = SaturatingRoundInt((long)target.Score + bonus);
                activeFamilySettlementContext.IndividualScore = SaturatingRoundInt((long)activeFamilySettlementContext.IndividualScore + bonus);
                RecordSettlementContribution(target, bonus, sourceSlot, true, true, "首领龟加分");
            }
            AddLog(DieDisplayName(target) + "依自身 " + bonus + " 枚龟壳获得 " + bonus + " 分。");
        }
    }

    private void RobRoundNeighbors(int slot, RoundScores state, bool commit)
    {
        long stolen = 0;
        int victims = 0;
        for (int i = slot - 1; i <= slot + 1; i += 2)
        {
            if (i < 0 || i >= dice.Count || dice[i].Temporary || state.Robbed[i]) continue;
            victims++;
            stolen += state.LockedPips[i];
            state.Robbed[i] = true;
            if (!commit) continue;

            Die target = dice[i];
            if (activeFamilySettlementContext.SettledIds.Contains(target.Id))
            {
                int previous = target.Score;
                activeFamilySettlementContext.IndividualScore = SaturatingRoundInt((long)activeFamilySettlementContext.IndividualScore - previous);
                RecordSettlementContribution(target, SaturatingRoundInt(-(long)previous), slot, true, true, "抢劫清零");
            }
            target.Score = 0;
            target.EffectiveValue = 0;
            target.RoundNote = AppendNote(target.RoundNote, "被抢劫，本手计零");
        }
        if (!commit || victims == 0) return;

        Die pirate = dice[slot];
        int faceCount = pirate.Faces.Length;
        long quotient = stolen / faceCount;
        long remainder = stolen % faceCount;
        for (int i = 0; i < faceCount; i++)
        {
            long gain = quotient + (i < Math.Abs(remainder) ? Math.Sign(remainder) : 0);
            pirate.Faces[i] = SaturatingRoundInt((long)pirate.Faces[i] + gain);
        }
        pirate.RoundNote = AppendNote(pirate.RoundNote, "抢劫 " + stolen + " 点，+3 金");
        MarkTypeTriggered(pirate);
        AddLog(DieDisplayName(pirate) + "均摊 " + stolen + " 点到六面，获得 3 金。" + AddGoldIncome(3, "抢劫海盗", true));
    }

    private void ApplyRoundEndEffects(HashSet<int> settled)
    {
        int passes = HasDiceOfType(DieType.Enchanter) ? 2 : 1;
        for (int pass = 0; pass < passes; pass++)
        {
            foreach (Die source in dice)
            {
                if (source.Temporary) continue;
                if (source.Type == DieType.SowPig)
                {
                    foreach (Die target in dice)
                    {
                        if (target != source) GrantFeed(target, 1, "母猪", source, true, settled, pass > 0);
                    }
                    if (dice.Count > 1) MarkTypeTriggered(source);
                }
                else if (source.Type == DieType.Lightfang)
                {
                    int growth = 2 * PresentMainFamilyCount();
                    if (growth == 0) continue;
                    foreach (Die target in dice)
                    {
                        if (!target.Temporary) AddValueToAllFaces(target, growth);
                    }
                    MarkTypeTriggered(source);
                    source.RoundNote = AppendNote(source.RoundNote, "光牙全袋六面 +" + growth);
                    AddLog("光牙使全袋每颗骰的六面各 +" + growth + "。");
                }
                DrainGameplayChainResponseWaves();
            }
        }
    }
}
