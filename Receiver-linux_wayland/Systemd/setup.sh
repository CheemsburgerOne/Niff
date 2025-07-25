#!/bin/bash

#Script creates a ydotooluser group and assigns root to it. 
#It creates a socket which ydotoold listens to and controls access to the ydotoold socket so that only desired users can interact with the socket.
#Script also adds permanent variable required by the deamon.

#Check if ydotooluser group exists, if not create it and assign to root
if ! getent group ydotooluser > /dev/null ; then
    groupadd ydotooluser
    usermode -aG ydotooluser
    newgrp ydotooluser
else
    echo "ydotooluser user already exists!"
fi

#Check if socket file exists and create it if necessary
if [ ! -d /var/ydotoold ]; then
    mkdir -p /var/ydotoold > /dev/null
    chown :ydotooluser /var/ydotoold
    chmod 770 /var/ydotoold
    chmod g+s /var/ydotoold
else
    echo "/var/ydotoold folder already exists!"
fi

#Check if folder for configuration exists and create it if necessary
if [ ! -d /etc/niff/remote ]; then
    mkdir -p /etc/niff/remote > /dev/null
    chown root:root /etc/niff/remote
    chmod 770 /etc/niff/remote
    chmod g+s /etc/niff/remotel;
else
    echo "/etc/niff folder already exists!"
fi

if ! grep --quiet 'YDOTOOL_SOCKET=' /etc/enviroment; then
    echo "export YDOTOOL_SOCKET=/var/ydotoold/ydotoold.sock" | tee -a /etc/environment > /dev/null
else
    echo "YDOTOOL_SOCKET variable already permanent!"
fi
