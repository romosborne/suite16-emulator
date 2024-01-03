## Running

On linux, use `start.sh` to automatically do the following.

### Manually

Use the following to create a loopback serial interface.

```bash
socat -d -d pty,raw,echo=0,link=/tmp/tty0 pty,raw,echo=0,link=/tmp/tty1
```

Then pass in one of these to the emulator

```bash
dotnet run -- /tmp/tty0
```
