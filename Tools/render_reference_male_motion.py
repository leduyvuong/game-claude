"""Package actual Unity motion captures as a captioned video, GIF and contact sheet."""
from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.tools/python'))
import numpy as np
from PIL import Image,ImageDraw,ImageFont
import imageio_ffmpeg
female='--female' in sys.argv
asset='ReferenceFemale' if female else 'ReferenceMale'
slug='reference-female' if female else 'reference-male'
source=ROOT/'.tools/model-work'/f'{slug}-frames'
out=ROOT/'Art/Previews'/asset/'Motion'
names=['Idle','Walk','Run','Wave','Crouch','Jump','Water','Hoe','Harvest','Carry','Plant']
labels=['Đứng nghỉ','Đi bộ tại chỗ','Chạy tại chỗ','Vẫy tay','Ngồi xuống','Nhảy','Tưới cây','Cuốc đất','Thu hoạch','Bê giỏ','Trồng cây']
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',26)
small=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',16)
def caption(im,label,index):
    im=im.convert('RGB').copy();draw=ImageDraw.Draw(im)
    draw.rectangle((0,0,im.width,63),fill='#293235')
    draw.text((18,5),label,font=font,fill='#f3ead8')
    subtitle='Model nữ · Unity' if female else 'Trang phục mới · Unity'
    draw.text((18,38),f'{index+1:02d}/11 · {subtitle}',font=small,fill='#bdcbc9')
    return im
video=imageio_ffmpeg.write_frames(str(out/(asset+'-clothed.mp4')),(512,640),fps=30,codec='libx264',pix_fmt_out='yuv420p',quality=8,output_params=['-movflags','+faststart'])
video.send(None)
for i,name in enumerate(names):
    files=sorted((source/name).glob('*.png'));assert len(files)>=40,(name,len(files))
    for file in files:video.send(np.asarray(caption(Image.open(file),labels[i],i)))
video.close()
sheet=Image.new('RGB',(4*384,3*480),'#293235')
for i,name in enumerate(names):
    im=caption(Image.open(out/(name+'.png')),labels[i],i).resize((384,480),Image.Resampling.LANCZOS)
    sheet.paste(im,((i%4)*384,(i//4)*480))
sheet.save(out/'All-actions.png')
selection=[1,2,3,6,7,8];frames=[]
counts={name:len(list((source/name).glob('*.png'))) for name in names}
for frame in range(0,144,3):
    im=Image.new('RGB',(3*256,2*320),'#293235')
    for cell,i in enumerate(selection):
        tile=caption(Image.open(source/names[i]/f'{frame % counts[names[i]]:03d}.png'),labels[i],i).resize((256,320),Image.Resampling.LANCZOS)
        im.paste(tile,((cell%3)*256,(cell//3)*320))
    frames.append(im)
palette=frames[0].quantize(colors=128)
frames=[f.quantize(palette=palette,dither=Image.Dither.NONE) for f in frames]
frames[0].save(out/'Clothed-preview.gif',save_all=True,append_images=frames[1:],duration=100,loop=0,optimize=False)
import shutil
shutil.copyfile(out/(asset+'-clothed.mp4'),out/(asset+'-motion.mp4'))
shutil.copyfile(out/'Clothed-preview.gif',out/'Motion-preview.gif')
print('Saved video, animated GIF and all-action contact sheet to',out)
