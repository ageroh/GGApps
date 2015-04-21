SELECT ententityid                                                     AS ID, 
       
	   /* entEntityTypeID, */ 
	   case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', title_s) ,0) = 0 then '' else
       isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', title_s), 0) as nvarchar(5)) + ' :</strong></br>', '///') end + 
	   CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', title_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(title_s, 1, 1) + '}' 
                     + Substring(title_s, 2, 1000) 
         ELSE Substring(title_s, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', title_s)-1) 
              + '{' 
              + Substring(title_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', title_s), 1) 
              + '}' 
              + Substring(title_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', title_s)+1, 
              1000) 
       END                                                             AS 
       TITLE, 
      
       case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', short_description_t),0) = 0 then '' else
       isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', short_description_t), 0) as nvarchar(5)) + ' :</strong></br>', '')  end  + 
	   CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', short_description_t), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(short_description_t, 1, 1) + '}' 
                     + Substring(short_description_t, 2, 1000) 
         ELSE Substring(short_description_t, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              short_description_t)-1) 
              + '{' 
              + Substring(short_description_t, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              short_description_t), 1) 
              + '}' 
              + Substring(short_description_t, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              short_description_t)+1, 1000) 
       END                                                             AS 
       [SHORT DESCRIPTION], 
       
       case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', body_t),0) = 0 then '' else
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', body_t), 0) as nvarchar(5)) + ' :</strong></br>', '')  end  + 
	   CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', body_t), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(body_t, 1, 1) + '}' 
                     + Substring(body_t, 2, 1000) 
         ELSE Substring(body_t, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', body_t)-1) 
              + '{' 
              + Substring(body_t, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', body_t), 1) 
              + '}' 
              + Substring(body_t, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', body_t)+1, 
              1000) 
       END                                                             AS BODY
       , 
       
       case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', caption_s),0) = 0 then '' else
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', caption_s), 0) as nvarchar(5)) + ' :</strong></br>', '') end  + 
	   CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', caption_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(caption_s, 1, 1) + '}' 
                     + Substring(caption_s, 2, 1000) 
         ELSE Substring(caption_s, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', caption_s) 
              -1) 
              + '{' 
              + Substring(caption_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', caption_s), 
              1) 
              + '}' 
              + Substring(caption_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', caption_s)+ 
              1, 1000 
              ) 
       END                                                             AS 
       CAPTION, 
       
       case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', opening_hours_s),0) = 0 then '' else
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', opening_hours_s), 0) as nvarchar(5)) + ' :</strong></br>', '') end  + 
       CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', opening_hours_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(opening_hours_s, 1, 1) + '}' 
                     + Substring(opening_hours_s, 2, 1000) 
         ELSE Substring(opening_hours_s, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              opening_hours_s)-1 
              ) 
              + '{' 
              + Substring(opening_hours_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              opening_hours_s), 1) 
              + '}' 
              + Substring(opening_hours_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              opening_hours_s)+1, 1000) 
       END                                                             AS 
       [OPENING HOURS], 
       
       case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', price_s),0) = 0 then '' else
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', price_s), 0) as nvarchar(5)) + ' :</strong></br>', '')  end + 
       CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', price_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(price_s, 1, 1) + '}' 
                     + Substring(price_s, 2, 1000) 
         ELSE Substring(price_s, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', price_s)-1) 
              + '{' 
              + Substring(price_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', price_s), 1) 
              + '}' 
              + Substring(price_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', price_s)+1, 
              1000) 
       END                                                             AS 
       PRICE, 

       case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', useful_tips_t),0) = 0 then '' else
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', useful_tips_t), 0) as nvarchar(5)) + ' :</strong></br>', '') end  + 
	   CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', useful_tips_t), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(useful_tips_t, 1, 1) + '}' 
                     + Substring(useful_tips_t, 2, 1000) 
         ELSE Substring(useful_tips_t, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              useful_tips_t 
              )-1) 
              + '{' 
              + Substring(useful_tips_t, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              useful_tips_t) 
              , 1) 
              + '}' 
              + Substring(useful_tips_t, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              useful_tips_t) 
              +1, 1000) 
       END                                                             AS 
       [USEFUL TIPS], 

		case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', address_s),0) = 0 then '' else
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', address_s), 0) as nvarchar(5)) + ' :</strong></br>', '') end  + 
       CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', address_s), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(address_s, 1, 1) + '}' 
                     + Substring(address_s, 2, 1000) 
         ELSE Substring(address_s, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', address_s) 
              -1) 
              + '{' 
              + Substring(address_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', address_s), 
              1) 
              + '}' 
              + Substring(address_s, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', address_s)+ 
              1, 1000 
              ) 
       END                                                             AS 
       [ADDRESS],
       
      case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', path_c),0) = 0 then '' else
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', path_c), 0) as nvarchar(5)) + ' :</strong></br>', '')  end + 
	   CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', path_c), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(path_c, 1, 1) + '}' 
                     + Substring(path_c, 2, 1000) 
         ELSE Substring(path_c, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', path_c)-1) 
              + '{' 
              + Substring(path_c, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', path_c), 1) 
              + '}' 
              + Substring(path_c, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', path_c)+1, 
              1000) 
       END                                                             AS [PATH] 
       , 
       
		case when isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', editorial_t),0) = 0 then '' else
	   isnull('<strong>At ' + cast(Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', editorial_t), 0) as nvarchar(5)) + ' :</strong></br>', '') end  + 
	   CASE Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', editorial_t), 0) 
         WHEN 0 THEN '' 
         WHEN 1 THEN '{' + Substring(editorial_t, 1, 1) + '}' 
                     + Substring(editorial_t, 2, 1000) 
         ELSE Substring(editorial_t, 1, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              editorial_t)-1) 
              + '{' 
              + Substring(editorial_t, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              editorial_t), 1) 
              + '}' 
              + Substring(editorial_t, Patindex('%[α-ωΑ-ΩΆ-Ώ]%', 
              editorial_t)+1, 
              1000) 
       END                                                            AS 
       EDITORIAL 
FROM   entity 
WHERE  Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', title_s), 0) 
       + Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', short_description_t), 0) 
       + Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', body_t), 0) 
       + Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', caption_s), 0) 
       + Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', opening_hours_s), 0) 
       + Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', price_s), 0) 
       + Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', useful_tips_t ), 0) 
       + Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', address_s), 0) 
       + Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', path_c), 0) 
       + Isnull(Patindex('%[α-ωΑ-ΩΆ-Ώ]%', editorial_t), 0) > 0 
       AND ententitytypeid > 0 
ORDER  BY 2, 
          1 