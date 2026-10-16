// Integrate imagegen cutouts as alpha-only edits; never import its repainted RGB.
// Run from repo root with WORDVENTURE_SHARP pointing to the installed sharp module.
const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const sharp = require(process.env.WORDVENTURE_SHARP || 'sharp');
const root = __dirname;
const frames = JSON.parse(fs.readFileSync(path.join(root,'frames.json')));
const apply = process.argv.includes('--apply');
const report = [];
function bbox(alpha) {
  let l=192,t=192,r=0,b=0,n=0;
  for(let y=0;y<192;y++)for(let x=0;x<192;x++)if(alpha[y*192+x]){l=Math.min(l,x);t=Math.min(t,y);r=Math.max(r,x);b=Math.max(b,y);n++;}
  return {cx:(l+r)/2,cy:(t+b)/2,n};
}
function components(a) {
  const seen=new Uint8Array(a.length), small=new Uint8Array(a.length);
  for(let p=0;p<a.length;p++)if(a[p]&&!seen[p]){
    const q=[p];seen[p]=1;
    for(let j=0;j<q.length;j++){
      const v=q[j],x=v%192,y=Math.floor(v/192);
      for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){
        const xx=x+dx,yy=y+dy,k=yy*192+xx;
        if(xx>=0&&xx<192&&yy>=0&&yy<192&&a[k]&&!seen[k]){seen[k]=1;q.push(k);}
      }
    }
    // Preserve isolated magic/snow/sap particles; the cleanup concerns body fringes.
    if(q.length<=12)for(const k of q)small[k]=1;
  }
  return small;
}
(async()=>{
  fs.mkdirSync('Logs/BossCutouts/preview',{recursive:true});
  for(const [name,list] of Object.entries(frames)){
    const generated=await sharp(path.join(root,name+'-generated.png')).resize(768,384,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();
    const previews=[];
    for(let index=0;index<list.length;index++){
      const f=list[index];
      const original=cp.execFileSync('git',['show','f9e0d79:'+f.file.replaceAll('\\','/')],{maxBuffer:5e6});
      const {data,info}=await sharp(original).ensureAlpha().raw().toBuffer({resolveWithObject:true});
      if(info.width!==f.width||info.height!==f.height)throw Error('Source dimensions changed');
      const old=new Uint8Array(192*192), candidate=new Uint8Array(192*192), points=[];
      for(let y=0;y<192;y++)for(let x=0;x<192;x++)candidate[y*192+x]=generated[(((Math.floor(index/4)*192+y)*768+(index%4)*192+x)*4)+3]>=200?1:0;
      for(let y=0;y<f.height;y++)for(let x=0;x<f.width;x++)if(data[(y*f.width+x)*4+3]){
        const xx=x+f.left,yy=y+f.top;old[yy*192+xx]=1;points.push([xx,yy]);
      }
      const ob=bbox(old),nb=bbox(candidate),keep=components(old);
      let best={score:-1};
      const initialX=Math.round(nb.cx-ob.cx),initialY=Math.round(nb.cy-ob.cy);
      for(const scale of [.94,.97,1,1.03,1.06])for(let dy=initialY-4;dy<=initialY+4;dy++)for(let dx=initialX-4;dx<=initialX+4;dx++){
        let intersection=0;
        for(const [x,y] of points){const xx=Math.round((x-ob.cx)/scale+ob.cx+dx),yy=Math.round((y-ob.cy)/scale+ob.cy+dy);if(xx>=0&&xx<192&&yy>=0&&yy<192&&candidate[yy*192+xx])intersection++;}
        const score=intersection/(points.length+nb.n*scale*scale-intersection);
        if(score>best.score)best={score,scale,dx,dy};
      }
      if(best.score<.72)throw Error(`${name} ${index}: cutout registration too different: ${best.score}`);
      const result=Buffer.from(data);let removed=0;
      for(const [x,y] of points){
        const k=y*192+x;if(keep[k])continue;
        let edge=false;
        for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++)if(!old[(y+dy)*192+x+dx])edge=true;
        if(!edge)continue;
        const xx=Math.round((x-ob.cx)/best.scale+ob.cx+best.dx),yy=Math.round((y-ob.cy)/best.scale+ob.cy+best.dy);
        if(xx>=0&&xx<192&&yy>=0&&yy<192&&candidate[yy*192+xx])continue;
        const p=((y-f.top)*f.width+x-f.left)*4;
        result.fill(0,p,p+4);removed++;
      }
      if(removed/points.length>.09)throw Error(`${name} ${index}: excessive edge removal`);
      const png=await sharp(result,{raw:{width:f.width,height:f.height,channels:4}}).png().toBuffer();
      const previewPath=path.join('Logs/BossCutouts/preview',`${name}-${String(index+1).padStart(2,'0')}-clean.png`);
      fs.writeFileSync(previewPath,png);
      if(apply&&removed)fs.writeFileSync(f.file,png);
      report.push({file:f.file,width:f.width,height:f.height,opaqueBefore:points.length,removed,opaqueRgbChanged:0,binaryAlpha:true,registration:best});
      for(let variant=0;variant<2;variant++)previews.push({input:variant?png:original,left:(index%4)*192+f.left,top:(Math.floor(index/4)+variant*2)*192+f.top});
    }
    const background=Buffer.from(`<svg width="768" height="768"><rect width="768" height="768" fill="#263247"/><rect x="384" width="384" height="768" fill="#e3ded1"/></svg>`);
    await sharp(background).composite(previews).png().toFile(path.join(root,name+'-before-after.png'));
  }
  fs.writeFileSync(path.join(root,'validation.json'),JSON.stringify(report,null,2)+'\n');
  console.log(JSON.stringify(Object.keys(frames).map(name=>({name,frames:report.filter(r=>r.file.replaceAll('\\','/').includes('/'+name+'/')).length,removed:report.filter(r=>r.file.replaceAll('\\','/').includes('/'+name+'/')).reduce((s,r)=>s+r.removed,0)}))));
})();
