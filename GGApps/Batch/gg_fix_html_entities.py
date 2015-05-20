import sqlite3, urllib, re, sys, socket
from datetime import datetime

from BeautifulSoup import BeautifulStoneSoup
import cgi

def H2U(text):
    """Converts HTML entities to unicode.  For example '&amp;' becomes '&'."""
    text = unicode(BeautifulStoneSoup(text, convertEntities=BeautifulStoneSoup.ALL_ENTITIES))
    return text

def U2H(text):
    """Converts unicode to HTML entities.  For example '&' becomes '&amp;'."""
    text = cgi.escape(text).encode('ascii', 'xmlcharrefreplace')
    return text

args = sys.argv

if  1 < len(args) < 4:
    city = args[1]
    if len(args) == 1:
        date = args[2]
    else:
        date = datetime.now().strftime("%Y%m%d")
else:
    raise SystemExit('No Aguments.')

lang = "RU"

socket.setdefaulttimeout(10)

db_path = './dbFiles/GreekGuide_%s_%s_%s.db' % (city,lang,date)


con = sqlite3.connect(db_path)

with con:
    sql="SELECT cat_id, cat_parent_id, cat_parent_name, cat_name FROM category_stats"
    u_sql="update category_stats set cat_parent_name=?, cat_name=? where cat_id=? and cat_parent_id=?"

    cur=con.cursor()
    cur.execute(sql)
    rows=cur.fetchall()
    
    for r in rows:
        cat_id=r[0]        
        cat_parent_id=r[1]

        cat_parent_name=H2U(r[2])
        cat_name=H2U(r[3])

        cur.execute(u_sql, (cat_parent_name, cat_name, cat_id, cat_parent_id))
        

    sql="select entEntityID, TITLE_S, SHORT_DESCRIPTION_T, BODY_T, OPENING_HOURS_S, PRICE_S, USEFUL_TIPS_T, ADDRESS_S, ent_hidden_BodyRaw, EDITORIAL_T, READMORE_ALIAS_S, SEEMORE_ALIAS_S, VIDEO_TITLE_S, LOCALITY_S from Entity"
    u_sql="update Entity set TITLE_S=?, SHORT_DESCRIPTION_T=?, BODY_T=?, OPENING_HOURS_S=?, PRICE_S=?, USEFUL_TIPS_T=?, ADDRESS_S=?, ent_hidden_BodyRaw=?, EDITORIAL_T=?, READMORE_ALIAS_S=?, SEEMORE_ALIAS_S=?, VIDEO_TITLE_S=?, LOCALITY_S=? where ententityid=?"

    cur=con.cursor()
    cur.execute(sql)
    rows=cur.fetchall()

    for r in rows:
        entEntityID=r[0]
        
        TITLE_S=H2U(r[1]) if r[1] is not None  else None
        SHORT_DESCRIPTION_T=H2U(r[2]) if r[2] is not None  else None
        BODY_T=H2U(r[3]) if r[3] is not None  else None
        OPENING_HOURS_S=H2U(r[4]) if r[4] is not None  else None
        PRICE_S=H2U(r[5]) if r[5] is not None  else None
        USEFUL_TIPS_T=H2U(r[6]) if r[6] is not None  else None
        ADDRESS_S=H2U(r[7]) if r[7] is not None  else None
        ent_hidden_BodyRaw=H2U(r[8]) if r[8] is not None  else None
        EDITORIAL_T=H2U(r[9]) if r[9] is not None  else None
        READMORE_ALIAS_S=H2U(r[10]) if r[10] is not None  else None
        SEEMORE_ALIAS_S=H2U(r[11]) if r[11] is not None  else None
        VIDEO_TITLE_S=H2U(r[12]) if r[12] is not None  else None
        LOCALITY_S=H2U(r[13]) if r[13] is not None  else None

        cur.execute(u_sql, (TITLE_S, SHORT_DESCRIPTION_T, BODY_T, OPENING_HOURS_S, PRICE_S, USEFUL_TIPS_T, ADDRESS_S, ent_hidden_BodyRaw, EDITORIAL_T, READMORE_ALIAS_S, SEEMORE_ALIAS_S, VIDEO_TITLE_S, LOCALITY_S, entEntityID))

    u_sql="update Entity set TITLE_S=replace(TITLE_S,'&amp;','&'), SHORT_DESCRIPTION_T=replace(SHORT_DESCRIPTION_T,'&amp;','&'), BODY_T=replace(BODY_T,'&amp;','&'), OPENING_HOURS_S=replace(OPENING_HOURS_S,'&amp;','&'), PRICE_S=replace(PRICE_S,'&amp;','&'), USEFUL_TIPS_T=replace(USEFUL_TIPS_T,'&amp;','&'), ADDRESS_S=replace(ADDRESS_S,'&amp;','&'), EDITORIAL_T=replace(EDITORIAL_T,'&amp;','&'), READMORE_ALIAS_S=replace(READMORE_ALIAS_S,'&amp;','&'), SEEMORE_ALIAS_S=replace(SEEMORE_ALIAS_S,'&amp;','&'), VIDEO_TITLE_S=replace(VIDEO_TITLE_S,'&amp;','&'), LOCALITY_S=replace(LOCALITY_S,'&amp;','&')"
    cur.execute(u_sql)
        
    sql="SELECT fltFilterID, fltName, fltShortName FROM Filter"
    u_sql="update Filter set fltName=?, fltShortName=? where fltFilterID=?"

    cur=con.cursor()
    cur.execute(sql)
    rows=cur.fetchall()

    for r in rows:
        fltFilterID=r[0]
        
        fltName=H2U(r[1]) 
        fltShortName=H2U(r[2]) 

        cur.execute(u_sql, (fltName, fltShortName, fltFilterID))

        
    sql="SELECT cat_parent_id, loc_parent_id, loc_id, loc_parent_name, loc_name FROM location_stats"
    u_sql="update location_stats set loc_parent_name=?, loc_name=? where cat_parent_id=? and loc_parent_id=? and loc_id=?"

    cur=con.cursor()
    cur.execute(sql)
    rows=cur.fetchall()
    
    for r in rows:
        cat_parent_id=r[0]
        loc_parent_id=r[1]
        loc_id=r[2]
        
        loc_parent_name=H2U(r[3])
        loc_name=H2U(r[4])

        cur.execute(u_sql, (loc_parent_name, loc_name, cat_parent_id, loc_parent_id, loc_id))
        
con.close()

print "Perormed HTML enities conversion for ", db_path


