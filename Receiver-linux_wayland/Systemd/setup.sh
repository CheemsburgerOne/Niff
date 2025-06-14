#!/usr/bin/bash
# Check if the ydotooluser group exists
if ! getent group ydotooluser > /dev/null 2>&1; then
  
    #If not, create usergroup that limits the access authorized to access the socket
    sudo groupadd ydotoolusers  > /dev/null 2>&1;
    
    #Add root and calling user to ydotoolusers group
    sudo adduser root ydotoolusers  > /dev/null 2>&1;
    sudo adduser root ydotoolusers  > /dev/null 2>&1;
    
    #Reload the shell
    sudo newgrp ydotoolusers  > /dev/null 2>&1;
else
  echo "Jest grupa"
fi;

# Check if the file exists
if [ ! -f "$2" ]; then
    echo "File '$FILE_PATH' exists."
else
    echo "File '$FILE_PATH' does not exist."
fi

#Create usergroup that limits the access authorized to access the socket
sudo groupadd ydotoolusers

#Add root and calling user to ydotoolusers group
sudo adduser root ydotoolusers
sudo adduser root ydotoolusers

#Reload the shell
newgrp ydotoolusers

#Create ydotoold folder and socket file
sudo mkdir /run/ydotoold
sudo touch /run/ydotoold/ydotoold.sock

sudo chgrp -hR ydotoolusers /run/ydotoold

echo 'export YDOTOOL_SOCKET="/run/ydotoold/ydotoold.sock"' > YDOTOOL_SOCKET="$HOME/.ydotool_socket"



