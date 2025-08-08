#!/usr/bin/env bash

case $1 in
start)
  if [ $# -lt 3 ]; then
      #Usage: $0 {start|stop} {socket-path} {socket-permissions}"
      exit 1
  fi
  echo "socket-path=$2.ydotoold_socket"
  echo "soccket-perm=$3 "
  setsid ydotoold --socket-path=$2.ydotoold_socket --socket-perm=$3 > out.log 2>&1 < /dev/null &
  exit 0
  ;;
stop)
  pkill ydotoold
  exit 0
  ;;
esac

exit 1