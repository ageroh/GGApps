select entEntityID, 
         ifnull(PATH_C, VIDEO_THUMB_PATH_C), 
         ententitytypeid, 
         new_path_c
 from Entity
 where new_path_c!=''
 /* and entEntityID in (7598) -- select * from entity_relation where  enrParentEntityID in (2484) */
 order by 1
