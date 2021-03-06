﻿//
// OnTrack Input/Output for CSV Files (comma seperated values)
//
//
// (C) by Boris Schneider, 2015

using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace OnTrack.IO.CSV
{

    /// <summary>
    /// CSV Reader Class
    /// </summary>

    public class Reader : IEnumerable <String[]>, IEnumerable 
    {
        private Antlr4.Runtime.ICharStream _input; // Input Stream
        private List<String> _header; // header list
        private List<String[]> _rows; // rows are dictionary of string, string
        private String _delimiter = ";"; // Optional Delimiter

        /// <summary>
        /// Creator from a string buffer
        /// </summary>
        /// <param name="buffer"></param>
        public Reader(String buffer, String delimiter = null)
        {
            _input = new Antlr4.Runtime.AntlrInputStream(buffer);
            if (delimiter != null) this.Delimiter = delimiter;
        }
        /// <summary>
        /// Creator from a text reader 
        /// </summary>
        /// <param name="buffer"></param>
        public Reader(System.IO.TextReader reader, String delimiter = null)
        {
            _input = new Antlr4.Runtime.AntlrInputStream(reader);
             if (delimiter !=null) this.Delimiter = delimiter;
        }
        /// <summary>
        /// Creator from a stream reader 
        /// </summary>
        /// <param name="buffer"></param>
        public Reader(System.IO.StreamReader reader, String delimiter = null)
        {
            _input = new Antlr4.Runtime.AntlrInputStream (reader);
             if (delimiter != null) this.Delimiter = delimiter;
        }
        /// <summary>
        /// get or sets the Delimiter to be used
        /// </summary>
        public String Delimiter { get { return _delimiter; } set { _delimiter = value; } }
        /// <summary>
        /// returns the Header of the CSV
        /// </summary>
        public List<String> Header { get { return _header; } }
        /// <summary>
        /// returns the Rows
        /// </summary>
        public List<String[]> Rows { get { return _rows; } }
        /// <summary>
        /// run the reader
        /// </summary>
        public bool Process()
        {
            if (_input != null)
            {
                try
                {
                    otCSVLexer _lexer = new otCSVLexer(_input);
                    _lexer.Delimiter = _delimiter;
                    // wrap a token-stream around the lexer
                    Antlr4.Runtime.CommonTokenStream tokens = new Antlr4.Runtime.CommonTokenStream(_lexer);
                    // create the aParser
                    otCSVParser aParser = new otCSVParser(tokens);
                    aParser.RemoveErrorListeners();
                    aParser.AddErrorListener(new ErrorListener());
                    otCSVParser .CsvbufferContext  aTree = aParser.csvbuffer();
                    // walk the parse tree
                    DataBuilder aListener = new DataBuilder(aParser);
                    Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(aListener, aTree);
                    // result
                    _header = aListener.Header;
                    _rows = aListener.Rows;
                    return true;

                } catch (Exception ex) {
                    return false;
                }

            }

            return false;
        }
        /// <summary>
        /// convert to String
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            StringBuilder aBuilder = new StringBuilder();
            // build the header
            if (_header != null)
            {
                for (int i = 0; i < _header.Count; i++)
                {
                    aBuilder.AppendFormat("[{0}]", _header[i]);
                    if (_header.Count > 0 && i < _header.Count - 1) aBuilder.Append("\t");
                }
                
                aBuilder.AppendLine();
            }
            // build the rows
            if (_rows != null)
            {
                foreach (String[] row in _rows)
                {
                    for (int i = 0; i < _header.Count; i++)
                    {
                       if (row.GetUpperBound (0) >= i) aBuilder.AppendFormat("{0}", row[i]);
                       if (_header.Count > 0 && i < _header.Count - 1) aBuilder.Append("\t");
                    }
                    aBuilder.AppendLine();
                }
            }
            return aBuilder.ToString();
        }
        public IEnumerator<String[]> GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        System.Collections.IEnumerator IEnumerable.GetEnumerator()
        {
            return _rows.GetEnumerator();
        }
    }
    /// <summary>
    /// ErrorListener
    /// </summary>
    public class ErrorListener : Antlr4.Runtime.BaseErrorListener
    {
        public override void ReportAmbiguity(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
        { }
        public override void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
        { }
        public override void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState) 
        { }
        /// <summary>
        /// process the SyntaxError
        /// </summary>
        /// <param name="recognizer"></param>
        /// <param name="offendingSymbol"></param>
        /// <param name="line"></param>
        /// <param name="charPositionInLine"></param>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public override  void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) 
        {
            if (charPositionInLine != 00) Console.WriteLine(String.Format("ERROR <{0},{1:D2}>:{2}", line, charPositionInLine, msg));
            else Console.WriteLine(String.Format("ERROR <line {0}>:{1}", line, msg));
        }
    }
    /// <summary>
    /// listener to create the data structure
    /// </summary>
    public class DataBuilder : otCSVBaseListener
    {
        private otCSVParser _parser;
        private List<String> _header; // header list
        private List<String> _currentRowFieldValues; // temp list for all the current row fields
        private List<String[]> _rows; // rows are list of String
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="parser"></param>
        public DataBuilder(otCSVParser parser)
        {
            _parser = parser;
        }
        /// <summary>
        /// returns the Header of the CSV
        /// </summary>
        public List<String> Header { get { return _header; } }
        /// <summary>
        /// returns the Rows
        /// </summary>
        public List<String[]> Rows {get { return _rows;} }
        /// <summary>
        /// Enters the Csvbuffer Rule -> new rows
        /// </summary>
        /// <param name="ctx"></param>
        public override void EnterCsvbuffer(otCSVParser.CsvbufferContext ctx)
        {
            _rows = new List<String[]>();
        }
        /// <summary>
        /// Exit the Header Rule -> build header
        /// </summary>
        /// <param name="ctx"></param>
        public override void ExitHeader(otCSVParser.HeaderContext ctx)
        {
            _header = new List<String>();
            for (int i = 0; i < _currentRowFieldValues.Count; i++)
            {
                String item = _currentRowFieldValues[i];
                // substitute
                if (String.IsNullOrEmpty(item)) item = '#' + (i+1).ToString ("D3");
                if (!_header.Contains(item)) _header.Add(item);
                else
                    _parser.NotifyErrorListeners("column name '" + item + "' defined more than once ");
            }
        }
        /// <summary>
        /// enter new row
        /// </summary>
        /// <param name="ctx"></param>
        public override void EnterRow([Antlr4.Runtime.Misc.NotNull] otCSVParser.RowContext context)
        {
            _currentRowFieldValues = new List<String>();
        }
        /// <summary>
        /// Exit the Row
        /// </summary>
        /// <param name="ctx"></param>
        public override void ExitRow(otCSVParser.RowContext ctx)
        {
            // return if we are leaving a header
            if (ctx.Parent.RuleIndex == otCSVParser.RULE_header)
            {
                // remove last element if this is null
                if (String.IsNullOrEmpty(_currentRowFieldValues[_currentRowFieldValues.Count - 1])) 
                    _currentRowFieldValues.RemoveAt(_currentRowFieldValues.Count - 1);
                return;
            }

            // else
             String[]  _row = null;

            // no _header ?
            if (_header.Count > 0)  _row = new String [_header.Count];
            int i = 0;

            // build and add - lose additional fields if we donot have them
            foreach (string v in _currentRowFieldValues){
                // resize if no header
                if (_header == null && (_row == null || _row.GetUpperBound(0) < i)) Array.Resize<String>(ref _row, i+1);
                // check array
                if (_row != null && _row.GetUpperBound(0) >= i)  _row[i] = v;
                i++;
            }
            // add the row
            _rows.Add(_row);
        }
        /// <summary>
        /// exit the TEXT Token
        /// </summary>
        /// <param name="ctx"></param>
        public override void ExitText(otCSVParser.TextContext ctx)
        {
            _currentRowFieldValues.Add(ctx.TEXT().GetText());
        }
        /// <summary>
        /// Exit the String Token
        /// </summary>
        /// <param name="ctx"></param>
        public override void ExitString(otCSVParser.StringContext ctx)
        {
            String aValue = ctx.STRING().GetText().Trim();
            aValue = aValue.TrimStart('"');
            aValue = aValue.TrimEnd('"');
            _currentRowFieldValues.Add(aValue);
        }
        /// <summary>
        /// Exit the Empty Token
        /// </summary>
        /// <param name="ctx"></param>
        public override void ExitEmpty(otCSVParser.EmptyContext ctx)
        {
            _currentRowFieldValues.Add(null);
        }
    }
}