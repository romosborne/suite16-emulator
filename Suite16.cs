using System.IO.Ports;

public class Suite16 : IDisposable {
    private readonly SerialPort _sp;
    private readonly List<string> _buffer;
    private readonly State _state;

    public Suite16(string port) {
        //var ports = SerialPort.GetPortNames();
        //Console.WriteLine($"Ports Available: {ports.Length}");
        //foreach(var port in ports) Console.WriteLine(port);

        _state = new State();

        _buffer = new List<string>();

        _sp = new SerialPort {
            PortName = port,
            BaudRate = 19200,
            DataBits = 8,
            StopBits = StopBits.One,
            Parity = Parity.None
        };

        _sp.DataReceived += new SerialDataReceivedEventHandler(Blah);
        _sp.Open();
        System.Console.WriteLine($"Opened port: {_sp.PortName}");
    }

    public void Dispose() {
        _sp.Close();
    }

    private void Blah(object sender, SerialDataReceivedEventArgs e) {
        try {
            var x = _sp.ReadExisting();
            // Console.WriteLine($"Read: {x}");
            _buffer.Add(x);
            if (x.EndsWith("\r")) {
                var command = string.Join("", _buffer);
                _buffer.Clear();
                Console.WriteLine(command);
                ExecuteCommand(command);

            }
        }
        catch (IOException ex) {
            Console.WriteLine(ex);
        }
    }

    private void Send(string ack) {
        //Console.WriteLine(ack);
        _sp.Write($"{ack}\r");
        Thread.Sleep(10);
    }

    private bool ExecuteCommand(string command) {
        if (!command.StartsWith('`')) return false;
        if (command.Length < 9) return false;

        var function1 = command.Substring(2, 2);
        var function2 = command.Substring(4, 2);
        var room = int.Parse(command.Substring(7, 2));
        System.Console.WriteLine($"f1: {function1}, f2: {function2}, r:{room}");

        switch (command[1]) {
            case 'S':
                switch (function1) {
                    case "AL":
                        switch (function2) {
                            case "OF":
                                foreach (var r in _state.Rooms) {
                                    r.On = false;
                                }
                                Send("`AXALOFG00");
                                break;
                        }
                        break;

                    case "IN":
                        switch (function2) {
                            case "UP":
                                _state.Rooms[room - 1].InputUp();
                                break;
                            case "DN":
                                _state.Rooms[room - 1].InputDown();
                                break;
                            default:
                                var input = int.Parse(function2);
                                _state.Rooms[room - 1].InputNumber = (input - 1);
                                break;
                        }
                        Send($"`AXAD{_state.Rooms[room - 1].InputNumber + 1:00}R{room:00}");
                        break;
                    case "MT":
                        switch (function2) {
                            case "OG":
                                _state.Rooms[room - 1].Mute = !_state.Rooms[room - 1].Mute;
                                break;
                            case "ON":
                                _state.Rooms[room - 1].Mute = true;
                                break;
                            case "OF":
                                _state.Rooms[room - 1].Mute = false;
                                break;
                            default: break;
                        }
                        Send($"`AXMT{(_state.Rooms[room - 1].Mute ? "ON" : "OF")}R{room:00}");
                        break;
                    case "RM":
                        if (function2 == "OF") {
                            _state.Rooms[room - 1].On = false;
                            Send($"`AXRMOFR{room:00}");
                        }
                        break;
                    case "VL":
                        switch (function2) {
                            case "UP":
                                _state.Rooms[room - 1].VolumeUp();
                                break;
                            case "DN":
                                _state.Rooms[room - 1].VolumeDown();
                                break;
                            default: break;
                        }
                        Send($"`AXV{_state.Rooms[room - 1].Volume:000}R{room:00}");
                        break;
                    case "V0":
                        _state.Rooms[room - 1].Volume = int.Parse(function2);
                        Send($"`AXV{_state.Rooms[room - 1].Volume:000}R{room:00}");
                        break;
                    case "B-":
                    case "B0":
                    case "B+":
                        _state.Rooms[room - 1].Bass = int.Parse(command.Substring(3, 3));
                        Send($"`AXB{_state.Rooms[room - 1].Bass:+00;-00;000}R{room:00}");
                        break;
                    case "T-":
                    case "T0":
                    case "T+":
                        _state.Rooms[room - 1].Treble = int.Parse(command.Substring(3, 3));
                        Send($"`AXT{_state.Rooms[room - 1].Treble:+00;-00;000}R{room:00}");
                        break;
                    default:
                        break;
                }
                break;
            case 'G':
                switch (function1) {
                    case "AL":
                        switch (function2) {
                            case "OF":
                                if (_state.Rooms.All(r => !r.On)) {
                                    Send("`AXALOFG00");
                                }
                                else {
                                    Send("`AXALONG00");
                                }
                                break;
                            case "RM":
                                Send($"`AXPAOFG00");
                                for (int i = 0; i < 16; i++) {
                                    Send($"`AXIT{i:00}G00");
                                }
                                for (int i = 0; i < 16; i++) {
                                    SendRoomUpdate(_state.Rooms[i], i + 1);
                                }
                                break;
                            default: break;
                        }
                        break;
                    case "IN":
                        if (!_state.Rooms[room - 1].On) Send($"`AXRMOFR{room:00}");
                        else Send($"`AXAD{_state.Rooms[room - 1].InputNumber + 1:00}R{room:00}");
                        break;
                    case "MT":
                        if (!_state.Rooms[room - 1].On) Send($"`AXRMOFR{room:00}");
                        else Send($"`AXMT{(_state.Rooms[room - 1].Mute ? "ON" : "OF")}R{room:00}");
                        break;
                    case "RM":
                        if (function2 == "OF") {
                            if (!_state.Rooms[room - 1].On) Send($"`AXRMOFR{room:00}");
                            else Send($"`AXAD{_state.Rooms[room - 1].InputNumber + 1:00}R{room:00}");
                        }
                        break;
                    case "VL":
                    case "V0":
                        Send($"`AXV{_state.Rooms[room - 1].Volume:000}R{room:00}");
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        return true;
    }

    private void SendRoomUpdate(Room r, int i) {
        Send($"`AXV{r.Volume:000}R{i:00}");
        Send($"`AXB{r.Bass:000}R{i:00}");
        Send($"`AXT{r.Treble:000}R{i:00}");
        Send($"`AXB{r.Balance:000}R{i:00}");
        if (!_state.Rooms[i - 1].On) Send($"`AXRMOFR{i:00}");
        else Send($"`AXAD{_state.Rooms[i - 1].InputNumber + 1:00}R{i:00}");
        Send($"`AXVPOFR{i:00}"); // Volume preset
        Send($"`AXMV{r.MaxVol}R{i:00}");
        Send($"`AXMT{(r.Mute ? "ON" : "OF")}R{i:00}");  // Mute
        Send($"`AXSTROR{i:00}"); // Stereo/monol/monor
        Send($"`AXLDOFR{i:00}"); // Loudness contour on/off
        Send($"`AXSEOFR{i:00}"); // Stereo enhance on/off
        Send($"`AV1000R{i:00}"); // Volume preset 1
        Send($"`AV2000R{i:00}"); // Volume preset 2
        Send($"`AV3000R{i:00}"); // Volume preset 3
        Send($"`AV4000R{i:00}"); // Volume preset 4
        Send($"`A1B000R{i:00}"); // Balance preset 1
        Send($"`A2B000R{i:00}"); // Balance preset 2
        for (int p = 1; i < 5; i++) {
            Send($"`AB{p}000R{i:00}"); // Tone preset x bass
            Send($"`AT{p}000R{i:00}"); // Tone preset x treble
            Send($"`ALD{p}OFR{i:00}"); // Tone preset x loudness
            Send($"`ALE{p}OFR{i:00}"); // Tone preset x stereo enhance
        }
        Send($"`AXPTC1R{i:00}"); // Party enable 1 (Set/Clear)
        Send($"`AXPTC1R{i:00}"); // Party enable 2
        Send($"`AXPTC1R{i:00}"); // Party enable 3
        Send($"`AXPTC1R{i:00}"); // Party enable 4
        Send($"`AXPV00R{i:00}"); // Page volume preset
        for (int page = 1; page < 9; page++) {
            Send($"`AXPGC{page}R{i:00}"); // Page enable (Set/Clear)
        }





    }
}