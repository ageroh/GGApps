SET NOCOUNT ON


select '*** Database Statistics for '+fltName from Filter where fltFilterID=(SELECT excValue FROM Setting where exsCode='CATEGORY')

select ""

select count(*) as "number of entities" from entity where ententitytypeid not in (0,24)

select ""

declare @stats_categories as varchar(max) = (select top 1 excValue from Setting where exsCode='STATS_CATEGORIES')

declare @dsql as varchar(max)=
'
SELECT [cat_parent_name] as Category, [number_of_entries]
  FROM [category_stats] 
  where  [cat_parent_id] in (#stats_categories#)
  and cat_name=''All''
  order by 1
'

set @dsql=replace(@dsql, '#stats_categories#', @stats_categories)

exec(@dsql)

select ""


--43	Accommodation
select row_number() over (order by title_s) as 'Num', e.title_s as 'Accomodation - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID 
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '43#%'
group by e.title_s


select ""

--45	Shopping & Beauty

select row_number() over (order by title_s) as 'Num', e.title_s as 'Shopping (& Beauty) - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID 
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '45#%'
group by e.title_s


select ""

--53	Beauty

select row_number() over (order by title_s) as 'Num', e.title_s as 'Beauty - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID 
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '53#%'
group by e.title_s


select ""


--46	Food
select row_number() over (order by title_s) as 'Num', title_s as 'Food - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID 
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '46#%'
group by e.title_s


select ""


--54	Nightlife
select row_number() over (order by title_s) as 'Num', title_s as 'Nightlife - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID 
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '54#%'
group by e.title_s

select ""

--160	Beaches & Beach Bars
select row_number() over (order by title_s) as 'Num', title_s as 'Beaches & Beach Bars - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID 
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '160#%'
group by e.title_s

select ""

--204	Gastronomy
select row_number() over (order by title_s) as 'Num', title_s as 'Gastronomy - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID 
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '204#%'
group by e.title_s

select ""

--264	Smart Tips
select row_number() over (order by title_s) as 'Num', title_s as 'Smart Tips - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID 
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '264#%'
group by e.title_s

select ""

--163	Gay Life
select row_number() over (order by title_s) as 'Num', title_s as 'Gay Life - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID 
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '163#%'
group by e.title_s


select ""

--43 Attractions
select row_number() over (order by title_s) as 'Num', e.title_s as 'Attraction - Best', min(f.fltName) as 'Type'
from entity e
inner join filter_entity b on e.entEntityID=b.fieEntityID and b.fieFilterID=20101
inner join filter_entity c on e.entEntityID=c.fieEntityID
inner join Filter f on f.fltFilterID=c.fieFilterID and f.fltCode like '44#%'
group by e.title_s


select ""