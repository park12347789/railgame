"""UI smoke tests. Uses an offline HTML document, with no network navigation.
Requires Python + Playwright + Chromium; browser location can be overridden.
The testing document enables the same debug API as ?test=1.
Real-origin localStorage persistence and native GPU rendering need manual verification.
"""
import json, os
from pathlib import Path
from playwright.sync_api import sync_playwright
ROOT=Path(__file__).resolve().parents[1]
HTML=(ROOT/'dist/index.html').read_text().replace("if(new URLSearchParams(location.search).has('test'))", "if(true)")
OUT=ROOT/'test-results';OUT.mkdir(exist_ok=True)

def state(page): return page.evaluate('window.__STACKRAIL__.getState()')
def click_stack(page,source,target):
 page.locator(f'[data-card="{source}"]').click()
 page.locator(f'[data-drop="{target}"]').click()
def drag_stack(page,source,target):
 src=page.locator(f'[data-card="{source}"]');dst=page.locator(f'[data-drop="{target}"]')
 src.scroll_into_view_if_needed();a=src.bounding_box();b=dst.bounding_box()
 page.mouse.move(a['x']+a['width']/2,a['y']+a['height']/2);page.mouse.down()
 page.mouse.move(b['x']+b['width']/2,b['y']+b['height']/2,steps=12);page.mouse.up()
 page.wait_for_timeout(320)

with sync_playwright() as p:
 browser=p.chromium.launch(executable_path=os.environ.get('CHROMIUM_PATH','/usr/bin/chromium'),headless=True,args=['--no-sandbox','--use-angle=swiftshader','--enable-unsafe-swiftshader'])
 page=browser.new_page(viewport={'width':1440,'height':1080},device_scale_factor=1)
 page.set_default_timeout(6000)
 errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
 page.set_content(HTML,wait_until='load')
 page.click('[data-action="start"]');page.click('[data-action="pause"]')
 assert page.evaluate('window.__STACKRAIL__.ui().paused')
 print('stage: staff drag',flush=True)
 drag_stack(page,'staff:0','freight');assert state(page)['workers'][0]=='freight'
 click_stack(page,'staff:1','tickets');click_stack(page,'staff:2','cafe')
 drag_stack(page,'resource:timber','freight');assert state(page)['jobs']['freight']['queue']==['timber']*3
 click_stack(page,'resource:passenger','tickets');click_stack(page,'resource:food','cafe')
 page.evaluate('window.__STACKRAIL__.advance(25)')
 assert state(page)['trains'][0]['load']==3
 print('stage: first train loaded',flush=True)
 before=state(page)['cash'];page.click('[data-action="depart"][data-id="101"]')
 assert state(page)['cash']==before+99
 assert state(page)['full']==1
 page.locator('.upgrade-button[data-id="platform"]').click();assert state(page)['platforms']==2
 click_stack(page,'resource:timber','freight');click_stack(page,'resource:passenger','tickets')
 page.evaluate('window.__STACKRAIL__.advance(46)')
 assert {t['lane'] for t in state(page)['trains'] if t['status']=='docked'}=={0,1}
 assert {t['kind'] for t in state(page)['trains']}=={'freight','passenger'}
 page.wait_for_timeout(500)
 print('stage: two platforms',flush=True)
 # Orbit and reset controls use real mouse input.
 angle=page.evaluate('window.__STACKRAIL__.scene().angle')
 a=page.locator('canvas').bounding_box();page.mouse.move(a['x']+a['width']/2,a['y']+a['height']/2);page.mouse.down();page.mouse.move(a['x']+a['width']/2+60,a['y']+a['height']/2,steps=8);page.mouse.up()
 assert page.evaluate('window.__STACKRAIL__.scene().angle')!=angle
 page.click('[data-action="camera"]');assert page.evaluate('window.__STACKRAIL__.scene().angle')==.48
 # Pause status must remain stable while assigning and expanding.
 assert page.evaluate('window.__STACKRAIL__.ui().paused')
 page.keyboard.press('Space');assert not page.evaluate('window.__STACKRAIL__.ui().paused')
 page.keyboard.press('Space');assert page.evaluate('window.__STACKRAIL__.ui().paused')
 page.wait_for_timeout(500);page.screenshot(path=str(OUT/'desktop.png'),full_page=True)
 renderer=page.evaluate('window.__STACKRAIL__.scene().cpu?"software-3d":"webgl"')
 assert page.evaluate('window.__STACKRAIL__.scene().triangles')>1000
 assert not errors,errors
 desktop=state(page)
 print('stage: mobile',flush=True)
 mobile=browser.new_page(viewport={'width':390,'height':844},device_scale_factor=1,is_mobile=True,has_touch=True)
 mobile.set_default_timeout(6000)
 mobile.on('pageerror',lambda e:errors.append('mobile: '+str(e)))
 mobile.set_content(HTML,wait_until='load');mobile.locator('[data-action="start"]').tap();mobile.locator('[data-action="pause"]').tap()
 mobile.locator('[data-card="staff:0"]').tap();mobile.locator('[data-drop="freight"]').tap()
 assert state(mobile)['workers'][0]=='freight'
 mobile.locator('[data-card="resource:timber"]').tap();mobile.locator('[data-drop="freight"]').tap()
 assert state(mobile)['jobs']['freight']['queue']==['timber']*3
 mobile.evaluate('window.__STACKRAIL__.advance(22)')
 assert state(mobile)['trains'][0]['load']==3
 mobile.locator('[data-action="depart"][data-id="101"]').tap();assert state(mobile)['full']==1
 assert mobile.evaluate('document.documentElement.scrollWidth')<=390
 mobile.evaluate('window.scrollTo(0,0)');mobile.wait_for_timeout(400);mobile.screenshot(path=str(OUT/'mobile.png'),full_page=True)
 assert not errors,errors
 report={'passed':True,'javascript_errors':errors,'renderer_tested':renderer,'desktop':{'width':1440,'drag':True,'click_stack':True,'depart':True,'two_platforms':True,'orbit':True,'keyboard_pause':True,'cash':desktop['cash'],'full_departures':desktop['full']},'mobile':{'viewport':[390,844],'tap_stack':True,'depart':True,'no_horizontal_overflow':True},'not_verified':['Native GPU WebGL path','Real-origin localStorage persistence','Physical Android/iOS devices'],'storage_note':'Opaque offline test document exercises the storage-unavailable fallback; pure simulation save/restore round-trip is covered by Node tests.'}
 (OUT/'browser-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2))
 print(json.dumps(report,ensure_ascii=False,indent=2));browser.close()
