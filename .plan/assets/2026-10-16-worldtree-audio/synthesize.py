"""Original deterministic cinematic SFX, no external samples. PCM16 mono 22050Hz."""
from pathlib import Path
import json
import wave
import uuid
import numpy as np

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'Assets/Resources/Combat/WorldTreeAudio'
EVIDENCE = Path(__file__).parent
RATE = 22050
rng = np.random.default_rng(12716)

def timeline(seconds):
    return np.arange(round(seconds * RATE)) / RATE

def lowpass(noise, width):
    return np.convolve(noise, np.ones(width) / width, mode='same')

def finish(signal):
    signal = signal / max(np.max(np.abs(signal)), 1e-9) * .78
    n = round(.006 * RATE)
    signal[:n] *= np.linspace(0, 1, n)
    signal[-n:] *= np.linspace(1, 0, n)
    return signal

def write(path, signal):
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), 'wb') as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(np.round(np.clip(signal, -1, 1) * 32767).astype('<i2').tobytes())

t = timeline(.6)
flash = sum(a * np.sin(2*np.pi*hz*t + .3*np.sin(2*np.pi*7*t)) * np.exp(-t/decay)
            for a,hz,decay in [(1,1320,.16),(.4,2134,.11),(.18,3520,.065)])
t = timeline(1.3)
noise = rng.normal(0,1,len(t))
approach = (.75*lowpass(noise,30) + .12*(noise-lowpass(noise,5)) + .09*np.sin(2*np.pi*(350*t+800*t*t)))
approach *= np.sin(np.pi*t/1.3)**.8
t = timeline(.8)
noise = rng.normal(0,1,len(t))
landing = .8*np.sin(2*np.pi*(48*t+10*(1-np.exp(-t*16))))*np.exp(-t*8)
landing += .5*lowpass(noise,24)*np.exp(-t*25)
landing += sum(.1*np.sin(2*np.pi*f*t)*np.exp(-t*12) for f in (147,231,371))
t = timeline(1.45)
noise = rng.normal(0,1,len(t))
rise = .18*lowpass(noise,16)*np.sin(np.pi*t/1.45)**1.6
for when,hz in [(0,523),(.2,659),(.4,784),(.65,1046),(.95,1318)]:
    age = np.maximum(0,t-when)
    rise += .12*np.sin(2*np.pi*hz*age)*np.exp(-age*5)*(t>=when)*np.minimum(1,age/.008)
clips = dict(Flash=finish(flash),Approach=finish(approach),Landing=finish(landing),CoreRise=finish(rise))
meta_template = '''fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 6
  defaultSettings:
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 22050
    compressionFormat: 0
    quality: 1
    conversionMode: 0
  platformSettingOverrides: {{}}
  forceToMono: 1
  normalize: 0
  preloadAudioData: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
'''
OUT.mkdir(parents=True, exist_ok=True)
if not OUT.with_suffix('.meta').exists():
    OUT.with_suffix('.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\n')
report=[]
for name,signal in clips.items():
    path=OUT/(name+'.wav')
    write(path,signal)
    meta=Path(str(path)+'.meta')
    if not meta.exists(): meta.write_text(meta_template.format(guid=uuid.uuid4().hex))
    assert signal[0]==0 and signal[-1]==0 and np.max(np.abs(signal))<.8
    report.append(dict(name=name,seconds=len(signal)/RATE,rate=RATE,peak=float(np.max(np.abs(signal))),rms=float(np.sqrt(np.mean(signal**2)))))

def mix(duration, cues):
    result=np.zeros(round(duration*RATE))
    for name,when,volume in cues:
        signal=clips[name].copy()
        if name=='Approach':
            signal=signal[:round(1.1*RATE)]
            progress=np.arange(len(signal))/(1.1*RATE)
            signal *= (.025+.625*progress**3)*np.minimum(1,(1-progress)/.06)
        else: signal*=volume
        offset=round(when*RATE)
        end=min(len(result),offset+len(signal))
        result[offset:end]+=signal[:end-offset]
    assert np.max(np.abs(result))<1
    return result
write(EVIDENCE/'entrance-preview.wav',mix(3.5,[('Flash',.85,.32),('Approach',1.05,1),('Landing',2.15,.65)]))
write(EVIDENCE/'core-preview.wav',mix(2.8,[('Landing',.3,.22),('CoreRise',1.05,.42),('Flash',1.7,.25)]))
(EVIDENCE/'signal-validation.json').write_text(json.dumps(report,indent=2)+'\n')
