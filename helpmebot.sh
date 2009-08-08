#!/bin/bash

#### THIS IS THE REAL CONTROL FILE!!!

COPY="hmb6"

cd /home/stwalkerster/$COPY

startbot() 
{
	rm bin/Helpmebot.$COPY.exe
	cp bin/Helpmebot.exe bin/Helpmebot.$COPY.exe
	mono bin/Helpmebot.$COPY.exe &> bin/log/`date +%Y-%m-%d--%H-%M` &
}

stopbot()
{
	PID=`ps -N -C "grep" -o pid,command | grep "bin/Helpmebot.$COPY.exe"  | awk -F" " '{ print $1 }' | tail -n 1`
	kill $PID 2> /dev/null
}

rebuildbot() 
{
	rm bin/Helpmebot.old.exe
	mv bin/Helpmebot.exe bin/Helpmebot.old.exe
	rm bin/MySql.Data.dll
	cp MySql.Data.dll bin/MySql.Data.dll
	rm bin/DotNetWikiBot.dll
	cp DotNetWikiBot.dll bin/DotNetWikiBot.dll
	gmcs -out:bin/Helpmebot.exe -reference:MySql.Data.dll -reference:DotNetWikiBot.dll -reference:/usr/lib/mono/2.0/System.Data.dll -reference:/usr/lib/mono/2.0/System.Web.dll -main:helpmebot6.Helpmebot6 Monitoring/WatcherController.cs Monitoring/CategoryWatcher.cs CommandParser.cs Configuration.cs ConfigurationSetting.cs DAL.cs GlobalFunctions.cs Helpmebot.cs IAL.cs NubioApi.cs User.cs WordLearner.cs
}

updatebot() 
{
	svn up
}

case $1 in
	start)
		startbot
	;;
	stop)
		stopbot
	;;
	force-restart)
		stopbot
		startbot
	;;
	restart)
		PID=`ps -N -C "grep" -o pid,command | grep "bin/Helpmebot.$COPY.exe"  | awk -F" " '{ print $1 }' | tail -n 1`
		if [ "$PID" = "" ]; then
			startbot	
        fi
	;;
	recompile)
		rebuildbot
	;;
	update)
		updatebot
		rebuildbot
		stopbot
		startbot
	;;
	scap)
		updatebot
	;;
	*)
		echo "Usage: helpmebot.sh {start|stop|restart|force-restart|recompile|update|scap}"
		exit 1
	;;
esac
