#!/usr/bin/env bash

echo "Creating virtual serial ports..."
socat -d -d pty,raw,echo=0,link=/tmp/tty0 pty,raw,echo=0,link=/tmp/tty1 &

echo "Starting emulator"
dotnet run -- /tmp/tty0

echo
echo "Closing ports and quitting"

trap "exit" INT TERM
trap "kill 0" EXIT
