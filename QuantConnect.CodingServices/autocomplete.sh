sudo killall mono
/etc/init.d/apache2 stop
cd /qc.server.autocomplete.worker/bin
nohup mono qc.server.autocomplete.worker.exe &
echo Autocomplete Loaded
