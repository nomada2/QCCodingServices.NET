# PROVIDED FOR EXAMPLE ONLY TO LOAD SERVICE

# Kill any excess.
sudo killall mono

# Stop the any servers on port 80 since ServiceStack will handle requests
/etc/init.d/apache2 stop

# Load 
cd /usr/bin/qc.server.autocomplete.worker/bin
nohup mono qc.server.autocomplete.worker.exe &

echo Autocomplete Loaded
