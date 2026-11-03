#!/bin/bash

case "$1" in
    webui)
        cd /opt/helpmebot
        exec ./Helpmebot.WebUI
        ;;
    bot)
        cd /opt/helpmebot
        shift
        exec ./Helpmebot "$@"
        ;;
    *)
        exec "$@"
        ;;
esac
