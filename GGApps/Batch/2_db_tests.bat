@echo off

if [%1]==[] goto usage
if [%2]==[] goto usage


echo ^<html^> > db_tests.html
echo ^<body^> >> db_tests.html
echo **************************************** ^</br^> >> db_tests.html
echo * Test 1 - entities without location ^</br^> >> db_tests.html
echo **************************************** ^</br^> >> db_tests.html
sqlcmd -S 10.0.64.32 -d ContentDB_165 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_test_2_1_entities_without_location.sql -y 8000 -o tmp.txt
type tmp.txt >> db_tests.html

echo **************************************** ^</br^> >> db_tests.html
echo * Test 1a - entities in multiple locations ^</br^> >> db_tests.html
echo **************************************** ^</br^> >> db_tests.html
sqlcmd -S 10.0.64.32 -d ContentDB_165_Lan_2_Cat_%1 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_test_2_1a_entities_in_multiple_locations.sql  -y 8000  -o tmp.txt
type tmp.txt >> db_tests.html

echo **************************************** ^</br^> >> db_tests.html
echo * Test 2 - entities connected to non leaves ^</br^> >> db_tests.html
echo **************************************** ^</br^> >> db_tests.html
sqlcmd -S 10.0.64.32 -d ContentDB_165_Lan_2_Cat_%1 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_test_2_2_entities_connected_to_non_leaves.sql -y 8000 -o tmp.txt
type tmp.txt >> db_tests.html

echo **************************************** ^</br^> >> db_tests.html
echo * Test 3 - entities without category ^</br^> >> db_tests.html
echo **************************************** ^</br^> >> db_tests.html
sqlcmd -S 10.0.64.32 -d ContentDB_165_Lan_2_Cat_%1 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_test_2_3_entities_without_category.sql -y 8000 -o tmp.txt
type tmp.txt >> db_tests.html

echo **************************************** ^</br^> >> db_tests.html
echo * Test 4a - entities with invalid characters (GR) ^</br^> >> db_tests.html
echo **************************************** ^</br^> >> db_tests.html
sqlcmd -S 10.0.64.32 -d ContentDB_165_Lan_1_Cat_%1 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_test_2_4_entities_with_invalid_characters_OK.sql -y 8000 -o tmp.txt
type tmp.txt >> db_tests.html

echo **************************************** ^</br^> >> db_tests.html
echo * Test 4b - entities with invalid characters (EN) ^</br^> >> db_tests.html
echo **************************************** ^</br^> >> db_tests.html
sqlcmd -S 10.0.64.32 -d ContentDB_165_Lan_2_Cat_%1 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_test_2_4_entities_with_invalid_characters_OK.sql -y 8000 -o tmp.txt
type tmp.txt >> db_tests.html

echo **************************************** ^</br^> >> db_tests.html
echo * Test 5 - entities with greek characters in english ^</br^> >> db_tests.html
echo **************************************** ^</br^> >> db_tests.html
sqlcmd -S 10.0.64.32 -d ContentDB_165_Lan_2_Cat_%1 -U ContentAbility_User_165 -P 3E6EA993-5EBA-4648-BF18-83C38D3E26DC -i db_test_2_5_entities_with_greek_characters_in_english_OK.sql -y 8000 -o tmp.txt
type tmp.txt >> db_tests.html

echo ^</body^> >> db_tests.html
echo ^</html^> >> db_tests.html


del tmp.txt

type db_tests.html > %2.html

goto end

:usage
@echo Usage: %0 ^<location_id^> ^<report_file^>
exit /B 1

:end