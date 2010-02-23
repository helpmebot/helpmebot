#!/bin/bash

#### THIS IS THE REAL CONTROL FILE!!!

COPY="hmb6"

cd /home/stwalkerster/$COPY

startbot() 
{
	job enable hmb6
}

stopbot()
{
	echo "Stopping bot..."
	job disable hmb6
}

rebuildbot() 
{
	echo "Rebuilding bot..."
	xbuild
}

updatebot() 
{
	mono ~/hmb-udp-sender.exe "#wikipedia-en-help :Bot going down for update."
	mono ~/hmb-udp-sender.exe "#wikipedia-en-accounts :Bot going down for update."
	mono ~/hmb-udp-sender.exe "##helpmebot :Bot going down for update."
	echo "Updating sourcecode from subversion"
	svn up
}

case $1 in
	start)
		startbot
	;;
	stop)
		stopbot
	;;
	restart)
		stopbot
		startbot
	;;
	update)
		updatebot
		rebuildbot
		stopbot
		startbot
	*)
		updatebot
		rebuildbot
		stopbot
		startbot
	;;
esac
