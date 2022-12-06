@echo off
set input=%1
set chars=a b c d e f g h i j k l m n o p q r s t u v w x y z
set n=4

:solve
set i=-1
:reset
set /a i=i+1
set j=%i%
set k=0
for %%x in (%chars%) do (
    set count[%%x]=0
)
:loop
call set "c=%%input:~%j%,1%%"
call set "p=%%count[%c%]%%"
if %p% neq 0 goto :reset
set count[%c%]=1
set /a k=k+1
set /a j=j+1
if %k% neq %n% goto :loop
echo First %n% unique at position %j%

if %n% neq 4 goto :end
set n=14
goto :solve

:end
