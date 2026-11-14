'use strict';
const fs=require('fs'),path=require('path'),assert=require('assert/strict');
const {chromium}=require('C:/Users/admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
(async()=>{
 const browser=await chromium.launch({headless:true,channel:'msedge'}),results=[];
 try{
  for(const entry of ['index.html','stress-template.html']){
   const page=await browser.newPage({viewport:{width:1950,height:1250}}),errors=[];page.on('pageerror',e=>errors.push(e.message));
   await page.goto('file:///'+path.join(__dirname,entry).replaceAll('\\','/'),{waitUntil:'domcontentloaded'});
   await page.waitForURL('**/index.html');await page.waitForFunction(()=>window.QA_READY||window.QA_ERROR);
   assert.equal(await page.evaluate(()=>window.QA_ERROR||null),null);
   assert.equal(await page.locator('#dice-count option').count(),9);assert.equal(await page.locator('#spell-count option').count(),9);
   for(const count of [4,6,8,32]){
    await page.locator('#dice-count').selectOption(String(count));await page.locator('#spell-count').selectOption(String(count));
    const s=await page.evaluate(()=>QA.snapshot());assert.equal(s.offers.filter(o=>o.category==='die').length,count);assert.equal(s.offers.filter(o=>o.category==='spell').length,count);assert.equal(s.pageCounts.die,Math.ceil(count/4));assert.equal(s.pageCounts.spell,Math.ceil(count/4));
   }
   await page.locator('#dice-count').selectOption('6');await page.locator('#spell-count').selectOption('6');
   assert.equal(await page.locator('[data-action="die-prev"]').isDisabled(),true);
   await page.locator('[data-action="die-next"]').click();let s=await page.evaluate(()=>QA.snapshot());assert.deepEqual(s.pages,{die:1,spell:0});assert.equal(await page.locator('[data-action="die-next"]').isDisabled(),true);
   await page.locator('[data-action="spell-next"]').click();s=await page.evaluate(()=>QA.snapshot());assert.deepEqual(s.pages,{die:1,spell:1});
   const last=s.visible.spell.at(-1);await page.locator(`[data-action="buy-${last.id}"]`).click();const bought=await page.evaluate(()=>QA.snapshot());assert.equal(bought.offers[last.index].kind,'empty');assert.deepEqual(bought.pages,s.pages);assert.deepEqual(bought.visible.spell.map(o=>o.id),s.visible.spell.map(o=>o.id));
   await page.locator('#refresh-test').click();s=await page.evaluate(()=>QA.snapshot());assert.deepEqual(s.pages,{die:0,spell:0});assert.ok(s.offers.every(o=>o.kind!=='empty'));
   await page.locator('#resolution').selectOption('1920');assert.equal(await page.locator('#screen').getAttribute('width'),'1920');
   const target=await page.evaluate(()=>QA.snapshot().visible.spell[0]);await page.locator(`[data-action="buy-${target.id}"]`).click();assert.equal((await page.evaluate(()=>QA.snapshot())).offers[target.index].kind,'empty');
   await page.locator('#resolution').selectOption('1280');assert.equal(await page.locator('#screen').getAttribute('width'),'1280');
   await page.locator('#dice-count').selectOption('8');await page.locator('#spell-count').selectOption('8');await page.locator('#refresh-test').click();
   assert.deepEqual(errors,[]);results.push({entry,redirectedTo:'index.html',diceOptions:9,spellOptions:9,automaticCounts:[4,6,8,32],independentPaging:true,lastPageEmptyStable:true,rebuild:true,resolutionAndHitboxes:[1280,1920],browserErrors:errors,passed:true});
   if(entry==='stress-template.html'){await page.setViewportSize({width:1320,height:950});await page.screenshot({path:path.join(__dirname,'entry-fixed.png'),fullPage:true});}
   await page.close();
  }
  const broken=await browser.newPage();await broken.addInitScript(()=>Object.defineProperty(window,'QA_DATA',{configurable:true,get(){return undefined;},set(){}}));await broken.goto('file:///'+path.join(__dirname,'index.html').replaceAll('\\','/'),{waitUntil:'domcontentloaded'});await broken.waitForFunction(()=>window.QA_ERROR);assert.match(await broken.locator('#status').textContent(),/页面初始化失败.*缺少已生成的测试数据/);results.push({entry:'缺失数据故障注入',visibleError:true,passed:true});await broken.close();
  fs.writeFileSync(path.join(__dirname,'entry-checks.json'),JSON.stringify({date:'2026-11-09',results},null,2));console.log(JSON.stringify({entryChecks:results},null,2));
 }catch(e){console.error(e.stack);process.exitCode=1;throw e;}finally{const timer=setTimeout(()=>process.exit(process.exitCode||0),3000);await browser.close();clearTimeout(timer);}
})().catch(e=>{console.error(e.stack);process.exit(1);});
