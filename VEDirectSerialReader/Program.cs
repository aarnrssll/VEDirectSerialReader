using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace VEDirectSerialReader
{
    public class Program
    {
        private static SerialPort _serialPort;

        // Exit app on exception.
        private static bool _exit { get; set; } = false;

        // SerialPort class supports the following encodings.
        private static readonly ASCIIEncoding _ascii = new ASCIIEncoding();
        //private static readonly UTF8Encoding _utf8 = new UTF8Encoding();
        //private static readonly UnicodeEncoding _unicode = new UnicodeEncoding();
        //private static readonly UTF32Encoding _ytf32 = new UTF32Encoding();

        // Message separators
        //private static readonly char _cr = '\r'; // Carriage return \x0D
        //private static readonly char _lf = '\n'; // Line feed \x0A
        //private static readonly char _ht = '\t'; // Horizontal Tab \x09

        public static void Main(string[] args)
        {
            try
            {
                // Instantiate the serial port
                _serialPort = new SerialPort()
                {
                    PortName = "COM1",
                    BaudRate = 19200,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    //NewLine = Environment.NewLine,
                    Encoding = _ascii,
                    //WriteBufferSize = 16,
                    //ReadTimeout = 1500,
                    //WriteTimeout = 1500,
                };

                // Open the serial port
                _serialPort.Open();

                // Subscribe to the DataReceived event
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedEventHandler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ ex.Message } \n");
                _exit = true;
            }

            // Run continuously handling "DataReceived" events.
            while (!_exit) { }

            Console.WriteLine("Press any key to exit the application.");
            Console.ReadLine();
        }

        private static void DataReceivedEventHandler(object sender, SerialDataReceivedEventArgs eventArgs)
        {
            // TODO: Temp to allow serial input buffer to fill. Device send data once per second. Change to stream or loop until end of message found.
            Thread.Sleep(750);

            try
            {
                // Read from the serial port buffer into byte array.
                var data = new byte[_serialPort.BytesToRead];
                _serialPort.Read(data, 0, data.Length);

                // TODO: Get string from ASCII encoded byte array and print. Why am I getting garbage???
                var message = _ascii.GetString(data);
                Console.WriteLine(message + "\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ ex.Message } \n");
                _exit = true;
            }
        }

        // TODO: Parse the message string into a class that represents the BMS.
        private static void ParseMessage(string message)
        {
            throw new NotImplementedException();
        }

        // TODO: Perform checksum calculation.
        private static bool IsValidMessage(string message)
        {
            throw new NotImplementedException();
        }
    }
}

/* Notes below taken from:
 * -----------------------
 * https://www.victronenergy.com/live/vedirect_protocol:faq
 * https://www.victronenergy.com/upload/documents/Whitepaper-Data-communication-with-Victron-Energy-products_EN.pdf
 * https://www.victronenergy.com/support-and-downloads/whitepapers Requires email
 * https://forum.arduino.cc/index.php?action=dlattach;topic=668006.0;attach=348859 Same doc as above with out email
 */

/* 
 * Message format:  <Newline><Field-Label><Tab><Field-Value>
 * ---------------------------------------------------------
 * <Newline>:       A carriage return followed by a line feed (0x0D, 0x0A).
 * <Field-Label>:   An arbitrary length label that identifies the field.
 * <Tab>:           A horizontal tab (0x09).
 * <Field-Value>:   The ASCII formatted value of this field. The number of characters transmitted depends on the magnitude and sign of the value.
 */

/* Implementation guidelines
 * -------------------------
 * When implementing a VE.Text parser it is recommended to reserve two buffers. For the field label a
 * buffer of 9 bytes is needed and for the field value a buffer length of 33 bytes is required. The value
 * should be parsed as soon as a single field is received and should then be stored in a temporary
 * record. The maximum number of fields in a block is 18; keep at least 18 temporary records. Once the
 * complete block is validated by evaluating the checksum, the contents of the temporary records can
 * be copied to its corresponding final records. If the checksum turned out to be invalid, the temporary
 * records need to be cleared.
*/

/* Serial port configuration:
 * --------------------------
 * Baud rate:       19200
 * Data bits:       8
 * Parity:          None
 * Stop bits:       1
 * Flow control:    None
 */

/* Pins to use when using the VE.Direct to RS232 interface:
 * --------------------------------------------------------
 * For the communication use the GND, RX and TX pins: pin 5, 2 and 3 on the DB9 connector.
 * Also the DTR signal (pin 4 on the DB9 connector) and/or the RTS signal (pin 7 on the DB9 connector)
 * must be driven high to power the isolated side of the interface. How to program the DTR and RTS
 * differs between used operating systems and hardware.
 */

/* Data integrity
 * --------------
 * The statistics are grouped in blocks with a checksum appended. The last field in a block will always
 * be “Checksum”. The value is a single byte, and will not necessarily be a printable ASCII character.
 * The modulo 256 sum of all bytes in a block will equal 0 if there were no transmission errors. Multiple
 * blocks are sent containing different fields.
 * https://www.victronenergy.com/live/vedirect_protocol:faq#q8how_do_i_calculate_the_text_checksum
 */