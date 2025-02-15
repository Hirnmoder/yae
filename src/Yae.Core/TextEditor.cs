﻿using System;
using System.IO;
using System.Threading.Tasks;
using Yae.Core.Templates;
using Yae.Core.Utils;
using Yae.Core.Validators;

namespace Yae.Core
{
    public sealed class TextEditor
    {
        private const int HeaderHeight = 3;
        private const int RowNumbersBarWidth = 9;
        private const int ScreenWidth = 150;


        private readonly string _name;
        private readonly FileInfo? _file;
        private readonly StreamReader? _sr;
        private readonly Func<StreamWriter>? _getSW;
        private readonly int _linesPerPage;
        private readonly TextBlock _textBlock;

        public TextEditor(FileInfo file, int linesPerPage)
        {
            LinesCountValidator.Validate(linesPerPage);
            FileValidator.Validate(file);

            _file = file;
            _name = file.FullName;
            _linesPerPage = linesPerPage;
            _textBlock = new TextBlock(linesPerPage);
        }

        public TextEditor(string name, StreamReader sr, Func<StreamWriter> getSW, int linesPerPage)
        {
            LinesCountValidator.Validate(linesPerPage);

            _linesPerPage = linesPerPage;
            _textBlock = new TextBlock(linesPerPage);

            _name = name;
            _sr = sr;
            _getSW = getSW;
        }

        public async Task RunAsync()
        {
            EnsureWindowSize();

            await InitAsync();

            var encoding = await (_file == default ? Task.FromResult(_sr.CurrentEncoding) : Source.GetEncodingAsync(_file));
            var inputData = await (_file == default ? Task.FromResult(_sr.ReadToEnd().Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.TrimEntries)) : Source.ReadAllLinesAsync(_file));

            Output.SetEncoding(encoding);
            await _textBlock.InitAsync(inputData);

            try
            {
                while (true)
                {
                    EnsureWindowSize();
                    await RenderAsync();
                    if (await WaitInputKeyAsync())
                    {
                        await CleanAsync();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                await CleanAsync();
                throw;
            }
        }

        private void EnsureWindowSize()
        {
            try
            {
                Console.SetWindowSize(ScreenWidth, _linesPerPage + 3 + 3 + 1);
            }
            catch
            {
            }
        }

        private async Task InitAsync()
        {
            Output.ClearScreen();

            await Layout.RenderHeaderAsync(_name, ScreenWidth);
            await Layout.RenderBodyAsync(_linesPerPage, ScreenWidth);
            await Layout.RenderFooterAsync(ScreenWidth);

            Output.MoveCursor(RowNumbersBarWidth, HeaderHeight);
        }

        private Task CleanAsync()
        {
            Output.MoveCursor(0, 0);
            Output.ClearScreen();
            return Task.CompletedTask;
        }

        private async Task RenderAsync()
        {
            Output.HideCursor();

            foreach (var currentLine in _textBlock.GetChangedLines())
            {
                Output.MoveCursor(0, currentLine.Row + HeaderHeight);
                await Layout.RenderLineAsync(currentLine.Value, currentLine.Number + 1);
                Output.MoveCursor(_textBlock.CursorX + RowNumbersBarWidth, _textBlock.CursorY + HeaderHeight);
            }

            Output.ShowCursor();
        }

        private Task SaveFileAsync()
        {
            if (_file == default)
            {
                return Source.SaveFileAsync(_getSW(), _textBlock.GetAllLines());
            }
            else
            {
                return Source.SaveFileAsync(_file, _textBlock.GetAllLines());
            }
        }

        private async Task<bool> WaitInputKeyAsync()
        {
            var inputKey = Input.ReadKey();

            switch (inputKey.Modifiers)
            {
                case ConsoleModifiers.Alt:
                    return false;
                case ConsoleModifiers.Control:
                    if (inputKey.Key == ConsoleKey.S)
                    {
                        await SaveFileAsync();
                    }

                    return false;
            }

            switch (inputKey.Key)
            {
                case ConsoleKey.Escape:
                    return true;
                case ConsoleKey.Backspace:
                    _textBlock.HandleBackspace();
                    break;
                case ConsoleKey.Enter:
                    _textBlock.HandleEnter();
                    break;
                case ConsoleKey.LeftArrow:
                    _textBlock.HandleLeftArrow();
                    break;
                case ConsoleKey.UpArrow:
                    _textBlock.HandleUpArrow();
                    break;
                case ConsoleKey.RightArrow:
                    _textBlock.HandleRightArrow();
                    break;
                case ConsoleKey.DownArrow:
                    _textBlock.HandleDownArrow();
                    break;
                case ConsoleKey.Delete:
                    _textBlock.HandleDelete();
                    break;
                case ConsoleKey.End:
                    _textBlock.HandleEnd();
                    break;
                case ConsoleKey.Home:
                    _textBlock.HandleHome();
                    break;
                case ConsoleKey.OemPlus:
                case ConsoleKey.OemComma:
                case ConsoleKey.OemMinus:
                case ConsoleKey.OemPeriod:
                case ConsoleKey.Oem1:
                case ConsoleKey.Oem2:
                case ConsoleKey.Oem3:
                case ConsoleKey.Oem4:
                case ConsoleKey.Oem5:
                case ConsoleKey.Oem6:
                case ConsoleKey.Oem7:
                case ConsoleKey.Oem8:
                case ConsoleKey.Oem102:
                case ConsoleKey.Spacebar:
                case ConsoleKey.D0:
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                case ConsoleKey.D6:
                case ConsoleKey.D7:
                case ConsoleKey.D8:
                case ConsoleKey.D9:
                case ConsoleKey.A:
                case ConsoleKey.B:
                case ConsoleKey.C:
                case ConsoleKey.D:
                case ConsoleKey.E:
                case ConsoleKey.F:
                case ConsoleKey.G:
                case ConsoleKey.H:
                case ConsoleKey.I:
                case ConsoleKey.J:
                case ConsoleKey.K:
                case ConsoleKey.L:
                case ConsoleKey.M:
                case ConsoleKey.N:
                case ConsoleKey.O:
                case ConsoleKey.P:
                case ConsoleKey.Q:
                case ConsoleKey.R:
                case ConsoleKey.S:
                case ConsoleKey.T:
                case ConsoleKey.U:
                case ConsoleKey.V:
                case ConsoleKey.W:
                case ConsoleKey.X:
                case ConsoleKey.Y:
                case ConsoleKey.Z:
                case ConsoleKey.NumPad0:
                case ConsoleKey.NumPad1:
                case ConsoleKey.NumPad2:
                case ConsoleKey.NumPad3:
                case ConsoleKey.NumPad4:
                case ConsoleKey.NumPad5:
                case ConsoleKey.NumPad6:
                case ConsoleKey.NumPad7:
                case ConsoleKey.NumPad8:
                case ConsoleKey.NumPad9:
                case ConsoleKey.Multiply:
                case ConsoleKey.Add:
                case ConsoleKey.Subtract:
                case ConsoleKey.Decimal:
                case ConsoleKey.Divide:
                    _textBlock.HandleAlphabet(inputKey);
                    break;
                default:
                    _textBlock.HandleSkippedKeys(inputKey);
                    break;
            }

            return false;
        }
    }
}