declare @sSql nvarchar(MAX)
DECLARE @EXsql NVARCHAR(MAX)

DECLARE @apptoex nvarchar(100)
DECLARE cursorName CURSOR -- Declare cursor
FAST_FORWARD FOR
	select '''0#2#'+ rtrim(ltrim(cast(catCategoryId as nchar))) + '#%'''
	from category  
	where catParentID = 2 
	order by catName
OPEN cursorName -- open the cursor
FETCH NEXT FROM cursorName INTO @apptoex
   --PRINT @fName + ' ' + @lName -- print the name
set @sSql = 'catcode like ' + @apptoex   
WHILE @@FETCH_STATUS = 0
BEGIN
	set @sSql = @sSql+ ' or catcode like ' + @apptoex
   FETCH NEXT FROM cursorName
   INTO @apptoex
END
CLOSE cursorName -- close the cursor
DEALLOCATE cursorName -- Deallocate the cursor

SET @EXsql = 'select 
	  ententityid as ID
	, entName as Name
from entity
where not exists
(select 1
from [Category_Entity] 
inner join Category 
on catCategoryID=[caeCategoryID]
where entEntityID=[caeEntityID]
and ('+@sSql+' ))
and entEntityTypeID>0
and entStatusID=3'

EXEC  sp_executesql @EXsql



