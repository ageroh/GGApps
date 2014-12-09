SELECT ententityid                                                     AS ID, 
       
	   /* entEntityTypeID, */ 
       isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', title_s), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
	   CASE Isnull(Patindex('%[�-��-٢-�]%', title_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(title_s, 1, 1) + '}' 
                     + Substring(title_s, 2, 1000) 
         ELSE Substring(title_s, 1, Patindex('%[�-��-٢-�]%', title_s)-1) 
              + '{' 
              + Substring(title_s, Patindex('%[�-��-٢-�]%', title_s), 1) 
              + '}' 
              + Substring(title_s, Patindex('%[�-��-٢-�]%', title_s)+1, 
              1000) 
       END                                                             AS 
       TITLE, 
      
       isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', short_description_t), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
	   CASE Isnull(Patindex('%[�-��-٢-�]%', short_description_t), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(short_description_t, 1, 1) + '}' 
                     + Substring(short_description_t, 2, 1000) 
         ELSE Substring(short_description_t, 1, Patindex('%[�-��-٢-�]%', 
              short_description_t)-1) 
              + '{' 
              + Substring(short_description_t, Patindex('%[�-��-٢-�]%', 
              short_description_t), 1) 
              + '}' 
              + Substring(short_description_t, Patindex('%[�-��-٢-�]%', 
              short_description_t)+1, 1000) 
       END                                                             AS 
       [SHORT DESCRIPTION], 
       
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', body_t), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
	   CASE Isnull(Patindex('%[�-��-٢-�]%', body_t), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(body_t, 1, 1) + '}' 
                     + Substring(body_t, 2, 1000) 
         ELSE Substring(body_t, 1, Patindex('%[�-��-٢-�]%', body_t)-1) 
              + '{' 
              + Substring(body_t, Patindex('%[�-��-٢-�]%', body_t), 1) 
              + '}' 
              + Substring(body_t, Patindex('%[�-��-٢-�]%', body_t)+1, 
              1000) 
       END                                                             AS BODY
       , 
       
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', caption_s), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
	   CASE Isnull(Patindex('%[�-��-٢-�]%', caption_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(caption_s, 1, 1) + '}' 
                     + Substring(caption_s, 2, 1000) 
         ELSE Substring(caption_s, 1, Patindex('%[�-��-٢-�]%', caption_s) 
              -1) 
              + '{' 
              + Substring(caption_s, Patindex('%[�-��-٢-�]%', caption_s), 
              1) 
              + '}' 
              + Substring(caption_s, Patindex('%[�-��-٢-�]%', caption_s)+ 
              1, 1000 
              ) 
       END                                                             AS 
       CAPTION, 
       
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', opening_hours_s), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
       CASE Isnull(Patindex('%[�-��-٢-�]%', opening_hours_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(opening_hours_s, 1, 1) + '}' 
                     + Substring(opening_hours_s, 2, 1000) 
         ELSE Substring(opening_hours_s, 1, Patindex('%[�-��-٢-�]%', 
              opening_hours_s)-1 
              ) 
              + '{' 
              + Substring(opening_hours_s, Patindex('%[�-��-٢-�]%', 
              opening_hours_s), 1) 
              + '}' 
              + Substring(opening_hours_s, Patindex('%[�-��-٢-�]%', 
              opening_hours_s)+1, 1000) 
       END                                                             AS 
       [OPENING HOURS], 
       
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', price_s), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
       CASE Isnull(Patindex('%[�-��-٢-�]%', price_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(price_s, 1, 1) + '}' 
                     + Substring(price_s, 2, 1000) 
         ELSE Substring(price_s, 1, Patindex('%[�-��-٢-�]%', price_s)-1) 
              + '{' 
              + Substring(price_s, Patindex('%[�-��-٢-�]%', price_s), 1) 
              + '}' 
              + Substring(price_s, Patindex('%[�-��-٢-�]%', price_s)+1, 
              1000) 
       END                                                             AS 
       PRICE, 

       
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', useful_tips_t), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
	   CASE Isnull(Patindex('%[�-��-٢-�]%', useful_tips_t), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(useful_tips_t, 1, 1) + '}' 
                     + Substring(useful_tips_t, 2, 1000) 
         ELSE Substring(useful_tips_t, 1, Patindex('%[�-��-٢-�]%', 
              useful_tips_t 
              )-1) 
              + '{' 
              + Substring(useful_tips_t, Patindex('%[�-��-٢-�]%', 
              useful_tips_t) 
              , 1) 
              + '}' 
              + Substring(useful_tips_t, Patindex('%[�-��-٢-�]%', 
              useful_tips_t) 
              +1, 1000) 
       END                                                             AS 
       [USEFUL TIPS], 

	   isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', address_s), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
       CASE Isnull(Patindex('%[�-��-٢-�]%', address_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(address_s, 1, 1) + '}' 
                     + Substring(address_s, 2, 1000) 
         ELSE Substring(address_s, 1, Patindex('%[�-��-٢-�]%', address_s) 
              -1) 
              + '{' 
              + Substring(address_s, Patindex('%[�-��-٢-�]%', address_s), 
              1) 
              + '}' 
              + Substring(address_s, Patindex('%[�-��-٢-�]%', address_s)+ 
              1, 1000 
              ) 
       END                                                             AS 
       [ADDRESS],
       
      
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', path_c), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
	   CASE Isnull(Patindex('%[�-��-٢-�]%', path_c), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(path_c, 1, 1) + '}' 
                     + Substring(path_c, 2, 1000) 
         ELSE Substring(path_c, 1, Patindex('%[�-��-٢-�]%', path_c)-1) 
              + '{' 
              + Substring(path_c, Patindex('%[�-��-٢-�]%', path_c), 1) 
              + '}' 
              + Substring(path_c, Patindex('%[�-��-٢-�]%', path_c)+1, 
              1000) 
       END                                                             AS [PATH] 
       , 
       

	   isnull('<strong>At ' + cast(Isnull(Patindex('%[�-��-٢-�]%', editorial_t), 0) as nvarchar(5)) + ' :</strong></br>', '') + 
	   CASE Isnull(Patindex('%[�-��-٢-�]%', editorial_t), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(editorial_t, 1, 1) + '}' 
                     + Substring(editorial_t, 2, 1000) 
         ELSE Substring(editorial_t, 1, Patindex('%[�-��-٢-�]%', 
              editorial_t)-1) 
              + '{' 
              + Substring(editorial_t, Patindex('%[�-��-٢-�]%', 
              editorial_t), 1) 
              + '}' 
              + Substring(editorial_t, Patindex('%[�-��-٢-�]%', 
              editorial_t)+1, 
              1000) 
       END                                                            AS 
       EDITORIAL 
FROM   entity 
WHERE  Isnull(Patindex('%[�-��-٢-�]%', title_s), 0) 
       + Isnull(Patindex('%[�-��-٢-�]%', short_description_t), 0) 
       + Isnull(Patindex('%[�-��-٢-�]%', body_t), 0) 
       + Isnull(Patindex('%[�-��-٢-�]%', caption_s), 0) 
       + Isnull(Patindex('%[�-��-٢-�]%', opening_hours_s), 0) 
       + Isnull(Patindex('%[�-��-٢-�]%', price_s), 0) 
       + Isnull(Patindex('%[�-��-٢-�]%', useful_tips_t ), 0) 
       + Isnull(Patindex('%[�-��-٢-�]%', address_s), 0) 
       + Isnull(Patindex('%[�-��-٢-�]%', path_c), 0) 
       + Isnull(Patindex('%[�-��-٢-�]%', editorial_t), 0) > 0 
       AND ententitytypeid > 0 
ORDER  BY 2, 
          1 