select 
entEntityID, 
entEntityTypeID, 
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',title_s),0)
when 0 then ''
when 1 then '{'+SUBSTRING(TITLE_S,1,1)+'}'+SUBSTRING(TITLE_S,2,1000)
else SUBSTRING(TITLE_S,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',title_s)-1)+'{'+SUBSTRING(TITLE_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',title_s),1)+'}'+SUBSTRING(TITLE_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',title_s)+1,1000)
end as TITLE_S,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',title_s),0) as pos1,
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',SHORT_DESCRIPTION_T),0)
when 0 then ''
when 1 then '{'+SUBSTRING(SHORT_DESCRIPTION_T,1,1)+'}'+SUBSTRING(SHORT_DESCRIPTION_T,2,1000)
else SUBSTRING(SHORT_DESCRIPTION_T,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',SHORT_DESCRIPTION_T)-1)+'{'+SUBSTRING(SHORT_DESCRIPTION_T,PATINDEX('%[á-ùÁ-Ù¢-¿]%',SHORT_DESCRIPTION_T),1)+'}'+SUBSTRING(SHORT_DESCRIPTION_T,PATINDEX('%[á-ùÁ-Ù¢-¿]%',SHORT_DESCRIPTION_T)+1,1000)
end as SHORT_DESCRIPTION_T,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',SHORT_DESCRIPTION_T),0) as pos2,
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',BODY_T),0)
when 0 then ''
when 1 then '{'+SUBSTRING(BODY_T,1,1)+'}'+SUBSTRING(BODY_T,2,1000)
else SUBSTRING(BODY_T,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',BODY_T)-1)+'{'+SUBSTRING(BODY_T,PATINDEX('%[á-ùÁ-Ù¢-¿]%',BODY_T),1)+'}'+SUBSTRING(BODY_T,PATINDEX('%[á-ùÁ-Ù¢-¿]%',BODY_T)+1,1000)
end as BODY_T,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',BODY_T),0) as pos3,
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',CAPTION_S),0)
when 0 then ''
when 1 then '{'+SUBSTRING(CAPTION_S,1,1)+'}'+SUBSTRING(CAPTION_S,2,1000)
else SUBSTRING(CAPTION_S,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',CAPTION_S)-1)+'{'+SUBSTRING(CAPTION_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',CAPTION_S),1)+'}'+SUBSTRING(CAPTION_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',CAPTION_S)+1,1000)
end as CAPTION_S,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',CAPTION_S),0) as pos4,
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',OPENING_HOURS_S),0)
when 0 then ''
when 1 then '{'+SUBSTRING(OPENING_HOURS_S,1,1)+'}'+SUBSTRING(OPENING_HOURS_S,2,1000)
else SUBSTRING(OPENING_HOURS_S,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',OPENING_HOURS_S)-1)+'{'+SUBSTRING(OPENING_HOURS_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',OPENING_HOURS_S),1)+'}'+SUBSTRING(OPENING_HOURS_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',OPENING_HOURS_S)+1,1000)
end as OPENING_HOURS_S,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',OPENING_HOURS_S),0) as pos5,
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',PRICE_S),0)
when 0 then ''
when 1 then '{'+SUBSTRING(PRICE_S,1,1)+'}'+SUBSTRING(PRICE_S,2,1000)
else SUBSTRING(PRICE_S,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',PRICE_S)-1)+'{'+SUBSTRING(PRICE_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',PRICE_S),1)+'}'+SUBSTRING(PRICE_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',PRICE_S)+1,1000)
end as PRICE_S,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',PRICE_S),0) as pos6,
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',USEFUL_TIPS_T),0)
when 0 then ''
when 1 then '{'+SUBSTRING(USEFUL_TIPS_T,1,1)+'}'+SUBSTRING(USEFUL_TIPS_T,2,1000)
else SUBSTRING(USEFUL_TIPS_T,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',USEFUL_TIPS_T)-1)+'{'+SUBSTRING(USEFUL_TIPS_T,PATINDEX('%[á-ùÁ-Ù¢-¿]%',USEFUL_TIPS_T),1)+'}'+SUBSTRING(USEFUL_TIPS_T,PATINDEX('%[á-ùÁ-Ù¢-¿]%',USEFUL_TIPS_T)+1,1000)
end as USEFUL_TIPS_T,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',USEFUL_TIPS_T),0) as pos7,
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',ADDRESS_S),0)
when 0 then ''
when 1 then '{'+SUBSTRING(ADDRESS_S,1,1)+'}'+SUBSTRING(ADDRESS_S,2,1000)
else SUBSTRING(ADDRESS_S,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',ADDRESS_S)-1)+'{'+SUBSTRING(ADDRESS_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',ADDRESS_S),1)+'}'+SUBSTRING(ADDRESS_S,PATINDEX('%[á-ùÁ-Ù¢-¿]%',ADDRESS_S)+1,1000)
end as ADDRESS_S,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',ADDRESS_S),0) as pos8,
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',PATH_C),0)
when 0 then ''
when 1 then '{'+SUBSTRING(PATH_C,1,1)+'}'+SUBSTRING(PATH_C,2,1000)
else SUBSTRING(PATH_C,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',PATH_C)-1)+'{'+SUBSTRING(PATH_C,PATINDEX('%[á-ùÁ-Ù¢-¿]%',PATH_C),1)+'}'+SUBSTRING(PATH_C,PATINDEX('%[á-ùÁ-Ù¢-¿]%',PATH_C)+1,1000)
end as PATH_C,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',PATH_C),0) as pos9,
case isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',EDITORIAL_T),0)
when 0 then ''
when 1 then '{'+SUBSTRING(EDITORIAL_T,1,1)+'}'+SUBSTRING(EDITORIAL_T,2,1000)
else SUBSTRING(EDITORIAL_T,1,PATINDEX('%[á-ùÁ-Ù¢-¿]%',EDITORIAL_T)-1)+'{'+SUBSTRING(EDITORIAL_T,PATINDEX('%[á-ùÁ-Ù¢-¿]%',EDITORIAL_T),1)+'}'+SUBSTRING(EDITORIAL_T,PATINDEX('%[á-ùÁ-Ù¢-¿]%',EDITORIAL_T)+1,1000)
end as EDITORIAL_T,isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',EDITORIAL_T),0) as pos10
from entity
where isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',title_s),0)+
isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',SHORT_DESCRIPTION_T),0)+
isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',BODY_T),0)+
isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',CAPTION_S),0)+
isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',OPENING_HOURS_S),0)+
isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',PRICE_S),0)+
isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',USEFUL_TIPS_T ),0)+
isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',ADDRESS_S),0)+
isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',PATH_C),0)+
isnull(PATINDEX('%[á-ùÁ-Ù¢-¿]%',EDITORIAL_T),0)>0
and ententitytypeid>0
order by 2, 1