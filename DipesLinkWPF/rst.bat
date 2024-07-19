@echo off
REM Wait for 2 seconds to ensure the application has shut down
ping 127.0.0.1 -n 3 > nul

REM Start the application again
call "%1"