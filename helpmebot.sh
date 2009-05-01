#!/bin/bash

#### THIS IS THE REAL CONTROL FILE!!!

COPY="hmb-ial"

cd /home/stwalkerster/$COPY

startbot() 
{
	rm helpmebot/runtime/Program.$COPY.exe
	cp helpmebot/runtime/Program.exe helpmebot/runtime/Program.$COPY.exe
	mono helpmebot/runtime/Program.$COPY.exe &> helpmebot/runtime/log/`date +%Y-%m-%d--%H-%M` &
}

stopbot()
{
	PID=`ps -N -C "grep" -o pid,command | grep "helpmebot/runtime/Program.$COPY.exe"  | awk -F" " '{ print $1 }' | tail -n 1`
	kill $PID 2> /dev/null
}

rebuildbot() 
{
	rm helpmebot/runtime/Program.old.exe
	mv helpmebot/runtime/Program.exe helpmebot/runtime/Program.old.exe
	gmcs -out:helpmebot/runtime/Program.exe -reference:helpmebot/MySql.Data.dll -reference:/usr/lib/mono/2.0/System.Data.dll -reference:/usr/lib/mono/2.0/System.Web.dll -main:helpmebot6.Helpmebot6 helpmebot/ApiCategoryParser.cs helpmebot/CommandParser.cs helpmebot/Configuration.cs helpmebot/ConfigurationSetting.cs helpmebot/DAL.cs helpmebot/GlobalFunctions.cs helpmebot/Helpmebot.cs helpmebot/IAL.cs helpmebot/NubioApi.cs helpmebot/User.cs helpmebot/WordLearner.cs
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
		PID=`ps -N -C "grep" -o pid,command | grep "helpmebot/runtime/Program.$COPY.exe"  | awk -F" " '{ print $1 }' | tail -n 1`
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
