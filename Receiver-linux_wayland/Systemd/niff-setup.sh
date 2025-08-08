#!/usr/bin/env bash

#Script creates a ydotooluser group and assigns root to it. 
#It creates a socket which ydotoold listens to and controls access to the ydotoold socket so that only desired users can interact with the socket.
#Script also adds permanent variable required by the deamon.

#Check if ydotooluser group exists, if not create it and assign to root
if ! getent group ydotooluser > /dev/null ; then
    groupadd ydotooluser
    newgrp ydotooluser
fi

#Check if socket file exists and create it if necessary
if [ -d /var/niff ]; then
  rm -rf /var/niff
fi

mkdir -p /var/niff/ > /dev/null
chown root:ydotooluser /var/niff/ 
chmod 070 /var/niff/ 
chmod g+s /var/niff/ 

#Check if folder for configuration exists and create it if necessary
if [ ! -d /etc/niff ]; then
  rm -rf /etc/niff
fi

mkdir -p /etc/niff/ > /dev/null
chown root:ydotooluser /etc/niff/ 
chmod 070 /etc/niff/ 
chmod g+s /etc/niff/
mkdir -p /etc/niff/remote > /dev/null
chmod 070 /etc/niff/remote/
mkdir -p /etc/niff/local > /dev/null
chmod 070 /etc/niff/local/
mkdir -p /etc/niff/translation > /dev/null
chmod 070 /etc/niff/translation/
mkdir -p /etc/niff/scripts > /dev/null
chmod 070 /etc/niff/scripts/
