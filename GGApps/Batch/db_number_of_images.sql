SET NOCOUNT ON

select '* fb images', (SELECT COUNT(*)
  FROM [Entity] where new_path_c!='' and entEntityTypeID not in (0,24))
  union
select '* images', (SELECT COUNT(*)
  FROM [Entity] where new_path_c!='' and entEntityTypeID not in (0,24))*2+
  (SELECT COUNT(*)
  FROM [Entity] where new_path_c!='' and entEntityTypeID=0)+
  (SELECT COUNT(*)
  FROM [Entity] where new_path_c!='' and entEntityTypeID=24)
  union
select '* max id',MAX(ententityid)FROM [Entity]  where new_path_c!='' 