'use strict';
const fs=require('fs'),path=require('path'),assert=require('assert/strict');
const {chromium}=require('C:/Users/admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const out=path.join(__dirname,'captures-illustrated');fs.mkdirSync(out,{recursive:true});
function overlaps(a,b){return a.x<b.x+b.w&&a.x+a.w>b.x&&a.y<b.y+b.h&&a.y+a.h>b.y;}
(async()=>{
 const browser=await chromium.launch({headless:true,channel:'msedge'}),checks=[],errors=[];
 try{
  const page=await browser.newPage({viewport:{width:1950,height:1250}});page.on('pageerror',e=>errors.push(e.message));await page.goto('file:///'+path.join(__dirname,'index.html').replaceAll('\\','/'),{waitUntil:'domcontentloaded'});await page.waitForFunction(()=>window.QA_READY||window.QA_ERROR);assert.equal(await page.evaluate(()=>window.QA_ERROR||null),null);
  for(const width of [1280,1920]){
   await page.evaluate(w=>QA.setResolution(w),width);
   for(let n=0;n<8;n++){
    await page.mouse.move(0,0);await page.evaluate(n=>{QA.setCase(8,8);if(n>=4)QA.next('spell');},n);
    const id='s'+n,locator=page.locator(`[data-card="${id}"]`),before=await page.evaluate(()=>QA.snapshot());assert.equal(before.tooltip,null);assert.ok(before.texts.every(t=>!t.id.endsWith('-rule')));
    await locator.evaluate(el=>{el.__stableHoverMarker='same-node';});await locator.hover();await page.waitForFunction(id=>QA.snapshot().tooltip?.id===id,id);const after=await page.evaluate(()=>QA.snapshot()),expected=await page.evaluate(n=>QA_DATA.spells[n].rule_text,n);assert.equal(after.tooltip.rule,expected);assert.ok(after.tooltip.texts.every(t=>t.fits&&!t.missing.length));const box=after.tooltip.box;assert.ok(box.x>=16&&box.y>=16&&box.x+box.w<=1264&&box.y+box.h<=704);assert.ok(after.buttons.filter(b=>b.meta.offerIndex!==undefined||b.id==='leave'||b.id==='refresh'||b.id==='spell-bag').every(b=>!overlaps(box,b.box)));assert.equal(await locator.evaluate(el=>el.__stableHoverMarker),'same-node');assert.equal(await page.locator('#spell-tooltip').evaluate(el=>getComputedStyle(el).pointerEvents),'none');
    const stage=await page.locator('#stage').boundingBox(),card=after.cards.find(c=>c.id===id),s=width/1280;for(const pos of [[card.box.x+20,card.box.y+20],[card.box.x+20,card.box.y+90],[card.box.x+30,card.box.y+152],[card.box.x+card.box.w-40,card.box.y+190]]){await page.mouse.move(stage.x+pos[0]*s,stage.y+pos[1]*s);assert.equal((await page.evaluate(()=>QA.snapshot())).tooltip?.id,id);}
    checks.push({resolution:width,spell:id,fullRule:true,inBounds:true,buttonsUnobscured:true,stableNode:true,moveWithinCardStable:true});
    if(n===3){await page.locator('#stage').screenshot({path:path.join(out,`hover-longest_${width}x${width===1280?720:1080}.png`)});}
   }
   await page.mouse.move(0,0);await page.evaluate(()=>QA.setCase(8,8));const key=page.locator('[data-card="s0"]');await key.focus();assert.equal((await page.evaluate(()=>QA.snapshot())).tooltip?.id,'s0');await page.keyboard.press('Escape');assert.equal((await page.evaluate(()=>QA.snapshot())).tooltip,null);await page.locator('#dice-count').focus();assert.equal((await page.evaluate(()=>QA.snapshot())).tooltip,null);checks.push({resolution:width,keyboardFocus:true,escape:true,blur:true});
   for(const action of ['page','rebuild','buy','leave','resize','replace']){
    await page.mouse.move(0,0);await page.evaluate(w=>{QA.setResolution(w);QA.setCase(8,8);},width);
    const pending=await page.evaluate(action=>{const c=QA.snapshot().cards[0],b=document.getElementById('screen').getBoundingClientRect(),s=QA.snapshot().width/1280;document.getElementById('stage').dispatchEvent(new PointerEvent('pointermove',{clientX:b.left+(c.box.x+20)*s,clientY:b.top+(c.box.y+60)*s,bubbles:true}));const start=QA.snapshot();if(action==='page')QA.next('spell');if(action==='rebuild')QA.refresh();if(action==='buy')QA.buy(c.index);if(action==='leave')QA.lock(true);if(action==='resize')QA.setResolution(QA.snapshot().width===1280?1920:1280);if(action==='replace')QA.replace(c.index);return {queued:start.pendingTooltip,visibleBefore:start.tooltip};},action);assert.equal(pending.queued,'s0');assert.equal(pending.visibleBefore,null);await page.waitForTimeout(160);const now=await page.evaluate(()=>QA.snapshot());assert.equal(now.tooltip,null);assert.equal(now.pendingTooltip,null);checks.push({resolution:width,cancelPending:action,passed:true});
   }
   await page.mouse.move(0,0);await page.evaluate(w=>{QA.setResolution(w);QA.setCase(8,8);QA.next('spell');},width);await page.locator('[data-card="s7"]').hover();await page.waitForFunction(()=>QA.snapshot().tooltip?.id==='s7');const before=await page.evaluate(()=>QA.snapshot());await page.locator('[data-action="buy-s7"]').click();const after=await page.evaluate(()=>QA.snapshot());assert.equal(after.lastAction.id,'s7');assert.equal(after.offers[before.tooltip.index].kind,'empty');assert.equal(after.tooltip,null);assert.equal(after.pages.spell,1);checks.push({resolution:width,hoverThenBuy:true,originalIndex:true,clearedAfterPurchase:true});
  }
  await page.mouse.move(0,0);await page.evaluate(()=>{QA.setResolution(1280);QA.setCase(8,8);});await page.locator('#stage').screenshot({path:path.join(out,'illustrated-front_1280x720.png')});assert.deepEqual(errors,[]);fs.writeFileSync(path.join(__dirname,'spell-hover-checks.json'),JSON.stringify({date:'2026-11-09',checks,errors},null,2));console.log(JSON.stringify({hoverChecks:checks.length,browserErrors:errors.length,passed:true},null,2));
 }catch(e){console.error(e.stack);process.exitCode=1;throw e;}finally{const timer=setTimeout(()=>process.exit(process.exitCode||0),3000);await browser.close();clearTimeout(timer);}
})().catch(e=>{console.error(e.stack);process.exit(1);});
