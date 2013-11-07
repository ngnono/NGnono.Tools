rem --rem --自动安装服务脚本
@echo 正在安装服务

sc create "NGnono.Tools.Scheduler.QuartzTest" binpath= "%~dp0..\NGnono.Tools.Scheduler" displayname= "NGnono.Tools.Scheduler.QuartzTest"
net start "NGnono.Tools.Scheduler.QuartzTest""

pause
