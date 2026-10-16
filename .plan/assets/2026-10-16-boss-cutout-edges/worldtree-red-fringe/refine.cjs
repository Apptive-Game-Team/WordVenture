const fs=require('fs'),cp=require('child_process'),path=require('path');
const sharp=require(process.env.WORDVENTURE_SHARP||'sharp');
const out=__dirname, apply=process.argv.includes('--apply');
const green=(r,g,b)=>g>r*1.05&&g>b*1.3&&g>45;
const warm=(r,g,b)=>r>=175&&g<155&&g<r*.63&&b<50;
(async()=>{
 const guide=await sharp(path.join(out,'generated.png')).resize(768,384,{kernel:'nearest'}).ensureAlpha().raw().toBuffer();
 const layers=[], report=[];
 for(let frame=0;frame<8;frame++){
  const file=`Assets/Art/Combat/Enemies/ActTwo/WorldTreeBoss/WorldTreeBossSlime_${String(frame+1).padStart(2,'0')}.png`;
  const before=cp.execFileSync('git',['show','521257d:'+file]);
  const {data,info}=await sharp(before).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const result=Buffer.from(data), w=info.width,h=info.height;
  let removed=0;
  // Follow imagegen's foliage regions but retain the original colors and art grid.
  // Only rusty colors touching foliage qualify; bright gold sap itself is outside this color range.
  for(let pass=0;pass<4;pass++){
   const remove=[];
   for(let y=0;y<h;y++)for(let x=0;x<w;x++){
    const p=(y*w+x)*4;
    if(!result[p+3]||!warm(result[p],result[p+1],result[p+2]))continue;
    let edge=false,leaves=0,guided=false;
    for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){
     const xx=x+dx,yy=y+dy;
     if(xx<0||yy<0||xx>=w||yy>=h){edge=true;continue;}
     const k=(yy*w+xx)*4;if(!result[k+3]){edge=true;continue;}
     if(green(result[k],result[k+1],result[k+2]))leaves++;
    }
    if(!edge||leaves<1)continue;
    const gx=(frame%4)*192+8+x,gy=Math.floor(frame/4)*192+10+y;
    for(let dy=-5;dy<=5&&!guided;dy++)for(let dx=-5;dx<=5;dx++){
     const xx=gx+dx,yy=gy+dy;if(xx<0||xx>=768||yy<0||yy>=384)continue;
     const k=(yy*768+xx)*4;if(guide[k+3]>=200&&green(guide[k],guide[k+1],guide[k+2])){guided=true;break;}
    }
    if(guided)remove.push(p);
   }
   for(const p of remove){result.fill(0,p,p+4);removed++;}
  }
  const after=await sharp(result,{raw:{width:w,height:h,channels:4}}).png().toBuffer();
  if(apply)fs.writeFileSync(file,after);
  for(let version=0;version<2;version++)layers.push({input:version?after:before,left:(frame%4)*192+8,top:(Math.floor(frame/4)+version*2)*192+10});
  if(frame===0){
   const detail=[];
   for(let v=0;v<2;v++)detail.push({input:await sharp(v?after:before).extract({left:0,top:0,width:176,height:95}).resize(1056,570,{kernel:'nearest'}).png().toBuffer(),left:0,top:v*570});
   await sharp({create:{width:1056,height:1140,channels:4,background:'#263247'}}).composite(detail).png().toFile(path.join(out,'canopy-before-after.png'));
  }
  report.push({file,width:w,height:h,removed,remainingRgbChanged:0});
 }
 const bg=Buffer.from('<svg width="768" height="768"><rect width="768" height="768" fill="#263247"/><rect x="384" width="384" height="768" fill="#e3ded1"/></svg>');
 await sharp(bg).composite(layers).png().toFile(path.join(out,'frames-before-after.png'));
 fs.writeFileSync(path.join(out,'validation.json'),JSON.stringify(report,null,2)+'\n');
 console.log(report.map(r=>r.removed));
})();
