import sqlite3, urllib, cStringIO, socket, sys
from PIL import Image, ImageDraw, ImageFont, ImageOps, ImageEnhance
from math import ceil
from time import sleep
from datetime import datetime


def reduce_opacity(im, opacity):
    """Returns an image with reduced opacity."""
    assert opacity >= 0 and opacity <= 1
    if im.mode != 'RGBA':
        im = im.convert('RGBA')
    else:
        im = im.copy()
    alpha = im.split()[3]
    alpha = ImageEnhance.Brightness(alpha).enhance(opacity)
    im.putalpha(alpha)
    return im

def watermark(im, mark, position, opacity=1):
    """Adds a watermark to an image."""
    if opacity < 1:
        mark = reduce_opacity(mark, opacity)
    if im.mode != 'RGBA':
        im = im.convert('RGBA')
    # create a transparent layer the size of the image and draw the
    # watermark in that layer.
    layer = Image.new('RGBA', im.size, (0,0,0,0))
    if position == 'tile':
        for y in range(0, im.size[1], mark.size[1]):
            for x in range(0, im.size[0], mark.size[0]):
                layer.paste(mark, (x, y))
    elif position == 'scale':
        # scale, but preserve the aspect ratio
        ratio = min(
            float(im.size[0]) / mark.size[0], float(im.size[1]) / mark.size[1])
        w = int(mark.size[0] * ratio)
        h = int(mark.size[1] * ratio)
        mark = mark.resize((w, h))
        layer.paste(mark, ((im.size[0] - w) / 2, (im.size[1] - h) / 2))
    else:
        layer.paste(mark, position)
    # composite the watermark with the layer
    return Image.composite(layer, im, layer)


def txt2img(frname,text):
    bg="#ffffff";fg="#000000";font="verdana.ttf";font_size=12
    font_dir = "C:\\Windows\\Fonts\\"
    fnt = ImageFont.truetype(font_dir+font, font_size)
    img=Image.open(fname)
    lineWidth = 16
    imgbg = Image.new('RGBA', img.size, "#000000") # make an entirely black image
    mask = Image.new('L',img.size,"#000000")       # make a mask that masks out all
    draw = ImageDraw.Draw(img)                     # setup to draw on the main image
    drawmask = ImageDraw.Draw(mask)                # setup to draw on the mask
    drawmask.line((0, lineWidth/2, img.size[0],lineWidth/2),
                  fill="#999999", width=16)        # draw a line on the mask to allow some bg through
    img.paste(imgbg, mask=mask)                    # put the (somewhat) transparent bg on the main
    draw.text((12,0), text, font=fnt, fill=bg)      # add some text to the main
    img.save(fname)
    del draw 


######################## SETTTINGS ####################
#city = "Athens"
#date = "20140722"
add_caption=False
facebook_thumbs=True
mark = Image.open('watermark3.png')
image_ratio=1.5
save_original=False
socket.setdefaulttimeout(5)

args = sys.argv

if  1 < len(args) < 4:
    city = args[1]
    if len(args) == 1:
        date = args[2]
    else:
        date = datetime.now().strftime("%Y%m%d")
else:
    raise SystemExit('No Aguments.')
    

db_path = 'C:\\inetpub\\wwwroot\\test.pos.gr\\GGApps\\Batch\\dbfiles\\GreekGuide_%s_EN_%s.db' % (city,date)
#db_path = 'm:/GreekGuide/Databases/Athens/GreekGuide_Athens_EN_20130902.db'
#db_path = 'm:/GreekGuide/Databases/Mykonos/GreekGuide_Mykonos_EN_20130902.db'
#db_path = 'm:/GreekGuide/Databases/Santorini/GreekGuide_Santorini_EL_20130820.db'
#db_path = 'M:/GreekGuide/FilesHistory/Databases/Athens/v1.1/ContentEN.db'


con = sqlite3.connect(db_path)

with open ("gg_img.sql", "r") as sqlfile:
    sql=sqlfile.read().replace('\n', '')

url_fmt=u'http://air.greekguide.com/cov/%s/%s_b.jpg'

fname_fmt="c:/temp/Images/%s/entity_%d_%s_%d_%d.jpg"
fname_fmt2="c:/temp/Images/%s-fb/entity_%d_%d_%d.jpg"

#fname_fmt="c:/temp/Images/Athens/entity_%d_%s_%d_%d.jpg"
#fname_fmt2="c:/temp/Images/Athens-fb/entity_%d_%d_%d.jpg"
#fname_fmt="c:/temp/Images/Mykonos/entity_%d_%s_%d_%d.jpg"
#fname_fmt2="c:/temp/Images/Mykonos-fb/entity_%d_%d_%d.jpg"
#fname_fmt="c:/temp/Images/Santorini/entity_%d_%s_%d_%d.jpg"
#fname_fmt2="c:/temp/Images/Santorini-fb/entity_%d_%d_%d.jpg"
#fname_fmt="M:/GreekGuide/FilesHistory/Images/%s/v1.1/entity_%d_%s_%d_%d.jpg"
#fname_fmt2="c:/%s/entity_%d_%d_%d.jpg"

#img_fmts=[ [1280,720,'JPEG',35], [960,640,'JPEG',35], [720,480,'JPEG',35], [220,220,'JPEG',35] ]
#img_fmts=[ [1280,720,'JPEG',45], [854,480,'JPEG',45], [720,480,'JPEG',45, 'C'], [720,480,'JPEG',45], [220,220,'JPEG',55] ]
#img_fmts=[  [720,480,'JPEG',45,'C',854,480,'W'], [220,220,'JPEG',55,'C',392,220] ]
#img_fmts=[  [640,426,'JPEG',35,'C',758,426,'W'], [220,220,'JPEG',45,'C',392,220]]
#img_fmts=[ [640,426,'JPEG',35,'W'], [608,352,'JPEG',35,'W'], [220,220,'JPEG',55]]
img_fmts=[ [[0,1,6,7,14],640,426,'JPEG',35,'W'], [[24],608,352,'JPEG',35,'W'], [[1,6,7,14],220,220,'JPEG',55]]
#img_fmts=[  [640,426,'JPEG',35,'W']]
#img_fmts=[ [1280,720,'JPEG',100] ]




with con:
    cur=con.cursor()
    cur.execute(sql)
    rows = cur.fetchall()

    for r in rows:
        id=r[0];path=r[1];imgtype=r[2];new_path=r[3]

                
        url = url_fmt % (path[:2],path)
        print "Retrieving id:%d %s" % (id,url),

        got_file=False
        
        while not got_file:
            try:            
                file = cStringIO.StringIO(urllib.urlopen(url.encode('utf-8')).read())
                img = Image.open(file)
                got_file=True             
            except IOError as i:
                print "- IOERROR", "(", i, ")",
                sleep(1)

        original_size=img.size
        print original_size,
        original_ratio=float(original_size[0])/original_size[1]
        print "- %.4f" % original_ratio

        if save_original:
            img.save(fname_fmt % (city,id,original_size[0],original_size[1]))


        for i in img_fmts:

#            if (not ((imgtype==0 or imgtype==24) and size==(220,220))) or (imgtype==24 and size==(608,352)):
#            if (imgtype!=24 and size==(640,426)) or (imgtype not in (0,24) and size==(220,220)) or (imgtype==24 and size==(608,352)) :
            if imgtype in i[0]:
                size=(i[1],i[2])
                image_ratio=float(size[0])/size[1]
                
                fname=fname_fmt % (city,id,new_path,size[0],size[1])
                fname2=fname_fmt2 % (city,id,size[0],size[1])


                if abs(original_ratio-image_ratio)<0.05:
                    img_r=img.resize(size, Image.ANTIALIAS)
                    print 'Resized - %s' % fname
                else:
                    if original_ratio>image_ratio:
                        stretch=(int(ceil(size[1]*original_ratio)),size[1])
                    else:
                        stretch=(size[0], int(ceil(size[0]*original_ratio)))
                    #img.thumbnail(size, Image.ANTIALIAS)
                    img_r=img.resize(stretch, Image.ANTIALIAS)
                    img_r = ImageOps.fit(img, size, Image.NEAREST, (0.5, 0.5))                    
                     #fname=fname.replace('.jpg','_C.jpg')
                    print 'Fitted using %s- %s' % (stretch,fname)


                if (len(i)==6 and i[5]=='W'):
                    img_r=watermark(img_r, mark, (img_r.size[0]-mark.size[0],img_r.size[1]-mark.size[1]), 0.8)

                img_r.save(fname,i[3], quality=i[4])

                if (facebook_thumbs and size==(220,220)):
                    img_r.save(fname2,i[3], quality=i[4])
                    

                if add_caption:
                    txt2img(fname, str(i))
con.close()
