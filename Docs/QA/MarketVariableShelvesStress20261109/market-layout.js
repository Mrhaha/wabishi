(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;else root.MarketLayout=api;})(typeof globalThis==='object'?globalThis:this,function(){
  'use strict';
  const CONFIG={width:1280,height:720,dicePerPage:4,spellsPerPage:4,gap:16,contentX:236,contentWidth:1008,dieY:128,dieWidth:240,dieHeight:204,spellY:390,spellWidth:240,spellHeight:224};
  function geometry(styles,name){const s=styles[name];return {...s,cell:16*s.pitch-(s.pitch-s.dot),line:16*s.pitch-(s.pitch-s.dot)+s.line_gap};}
  function advance(ch,glyphs,g){if(ch==='\t')return g.pitch*6;if(/\s/u.test(ch))return g.pitch*3;return (glyphs[ch]?.width??8)*g.pitch-(g.pitch-g.dot)+g.character_gap;}
  function measure(text,glyphs,g){return Math.max(0,[...text].reduce((n,c)=>n+advance(c,glyphs,g),0)-g.character_gap);}
  function wrap(text,width,glyphs,g){const result=[];for(const source of text.replace(/\r/g,'').split('\n')){if(!source){result.push('');continue;}let current='';for(const ch of source){const candidate=current+ch;if(current&&measure(candidate,glyphs,g)>width){result.push(current.trimEnd());current=ch;}else current=candidate;}result.push(current.trimEnd());}return result;}
  function metrics(text,width,glyphs,g,wrapped){const lines=wrapped?wrap(text,width,glyphs,g):text.replace(/\r/g,'').split('\n');return {lines,width:Math.max(0,...lines.map(l=>measure(l,glyphs,g))),height:g.cell+Math.max(0,lines.length-1)*g.line};}
  function group(offers,category){return offers.filter(o=>o.category===category);}
  function pageCount(items,capacity){return Math.max(1,Math.ceil(items.length/capacity));}
  function visible(items,page,capacity){return items.slice(page*capacity,(page+1)*capacity);}
  function createOffers(data,diceCount,spellCount,longPrices){const offers=[];let d=0,s=0;while(d<diceCount||s<spellCount){if(d<diceCount){const die=data.dice[d%data.dice.length];offers.push({id:'d'+d,index:offers.length,category:'die',kind:'die',name:die.display_name,price:longPrices?9999:Number(die.buy_price),asset:die.asset});d++;}if(s<spellCount){const spell=data.spells[s%data.spells.length];offers.push({id:'s'+s,index:offers.length,category:'spell',kind:'spell',name:spell.display_name,rule:spell.rule_text,price:longPrices?9999:Number(spell.price),spellId:spell.id});s++;}}return offers;}
  function markEmpty(offers,index){const offer=offers[index];if(!offer||offer.kind==='empty')return false;offer.kind='empty';return true;}
  function changeToDie(offers,index,die){const offer=offers[index];if(!offer||offer.kind==='empty')return false;Object.assign(offer,{category:'die',kind:'die',name:die.display_name,price:Number(die.buy_price),asset:die.asset});delete offer.rule;delete offer.spellId;return true;}
  return {CONFIG,geometry,advance,measure,wrap,metrics,group,pageCount,visible,createOffers,markEmpty,changeToDie};
});
