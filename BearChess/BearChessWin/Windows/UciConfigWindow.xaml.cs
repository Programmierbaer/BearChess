﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessDatabase;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für UciConfigWindow.xaml
    /// </summary>
    public partial class UciConfigWindow : Window
    {
        private readonly UciInfo _uciInfo;
        private readonly bool _showButtons;

        public bool SaveAsNew { get; private set; }


        public class ButtonConfigEventArgs : EventArgs
        {
            public string EngineName { get; }
            public string ConfigCmd { get; }

            public ButtonConfigEventArgs(string engineName, string configCmd)
            {
                EngineName = engineName;
                ConfigCmd = configCmd;
            }
        }

        public event EventHandler<ButtonConfigEventArgs> ButtonConfigEvent;

        public UciConfigWindow()
        {
            InitializeComponent();
        }

        public UciConfigWindow(UciInfo uciInfo,  bool canChangeName, bool showButtons, bool canSaveAs) : this()
        {
            Title += $" {uciInfo.OriginName}";
            _uciInfo = uciInfo;
            _showButtons = showButtons;
            textBlockName.ToolTip = uciInfo.OriginName;
            textBoxName.Text = uciInfo.Name;
            textBoxName.IsEnabled = canChangeName;
            buttonSaveAs.Visibility = canSaveAs ? Visibility.Visible : Visibility.Hidden;
            if (!canChangeName)
            {
                textBoxName.ToolTip = uciInfo.Name;
            }
            else
            {
                textBoxName.ToolTip = "Name of the configuration";
            }
            textBlockFileName.Text = uciInfo.FileName;
            textBlockFileName.ToolTip = uciInfo.FileName;
            var installedBooks = OpeningBookLoader.GetInstalledBooks();
            comboBoxOpeningBooks.ItemsSource = installedBooks;
            comboBoxOpeningBooks.SelectedIndex = 0;
            radioButtonBest.IsEnabled = false;
            radioButtonFlexible.IsEnabled = false;
            radioButtonWide.IsEnabled = false;
            OpeningBook.VariationsEnum variation = (OpeningBook.VariationsEnum)int.Parse(uciInfo.OpeningBookVariation);
            radioButtonBest.IsChecked = variation == OpeningBook.VariationsEnum.BestMove;
            radioButtonFlexible.IsChecked = variation == OpeningBook.VariationsEnum.Flexible;
            radioButtonWide.IsChecked = variation == OpeningBook.VariationsEnum.Wide;
            checkBoxUseOpeningBook.IsEnabled = installedBooks.Length > 0;
            if (!string.IsNullOrWhiteSpace(uciInfo.OpeningBook))
            {
                for (int i = 0; i < installedBooks.Length; i++)
                {
                    if (installedBooks[i].Equals(uciInfo.OpeningBook))
                    {
                        comboBoxOpeningBooks.SelectedIndex = i;
                        checkBoxUseOpeningBook.IsChecked = true;
                        break;
                    }
                }
            }
            int optionsLength = uciInfo.Options.Length / 2;
            int count = 0;
            int rowIndex = 0;
            foreach (var option in uciInfo.Options)
            {
                if (option.StartsWith("option name UCI_EngineAbout", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (option.StartsWith("option name Licensed To", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var currentValue = string.Empty;
                foreach (string optionValue in uciInfo.OptionValues)
                {
                    var optionSplit = optionValue.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (optionSplit.Length < 3
                        || !optionSplit[0].Equals("setoption", StringComparison.OrdinalIgnoreCase)
                        || !optionSplit[1].Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var oName = string.Empty;
                    for (int i = 2; i < optionSplit.Length - 2; i++)
                    {
                        oName = oName + optionSplit[i] + " ";
                    }
                    if (option.StartsWith($"option name {oName.Trim()} type"))
                    {
                        currentValue = optionSplit[optionSplit.Length - 1];
                        break;
                    }
                }

                count++;
                int colIndex = count > optionsLength ? 0 : 2;
                AddControl(option, currentValue, colIndex, rowIndex);
                rowIndex++;
                if (rowIndex > optionsLength)
                {
                    rowIndex = 0;
                }
            }
        }

        private string GetVariation()
        {
            if (radioButtonBest.IsChecked.HasValue && radioButtonBest.IsChecked.Value)
            {
                return ((int)OpeningBook.VariationsEnum.BestMove).ToString();
            }
            if (radioButtonFlexible.IsChecked.HasValue && radioButtonFlexible.IsChecked.Value)
            {
                return ((int)OpeningBook.VariationsEnum.Flexible).ToString();
            }

            return ((int)OpeningBook.VariationsEnum.Wide).ToString();
        }
        
        public UciInfo GetUciInfo()
        {
            var uciInfo = new UciInfo(_uciInfo.FileName)
            {
                Id = SaveAsNew ? "uci"+Guid.NewGuid().ToString("N") : _uciInfo.Id,
                Author = _uciInfo.Author,
                Name = string.IsNullOrWhiteSpace(textBoxName.Text) ? _uciInfo.OriginName : textBoxName.Text,
                OriginName = _uciInfo.OriginName,
                Valid = _uciInfo.Valid,
                OpeningBook = checkBoxUseOpeningBook.IsChecked.HasValue && checkBoxUseOpeningBook.IsChecked.Value ? comboBoxOpeningBooks.SelectedItem.ToString() : string.Empty,
                OpeningBookVariation = GetVariation()
            };
            foreach (var uciInfoOption in _uciInfo.Options)
            {
                uciInfo.AddOption(uciInfoOption);
            }
            foreach (var uciConfigValue in GetValues())
            {
                if (!string.IsNullOrWhiteSpace(uciConfigValue.CurrentValue))
                {
                    uciInfo.AddOptionValue(
                        $"setoption name {uciConfigValue.OptionName.Trim()} value {uciConfigValue.CurrentValue}");
                }
            }

            return uciInfo;
        }

        public UciConfigValue[] GetValues()
        {
            List<UciConfigValue> result = new List<UciConfigValue>();
            foreach (UIElement gridMainChild in gridMain.Children)
            {
                if (!(gridMainChild is IUciConfigUserControl))
                {
                    continue;
                }

                IUciConfigUserControl uciConfigUserControl = gridMainChild as IUciConfigUserControl;
                result.Add(uciConfigUserControl.ConfigValue);
            }

            return result.ToArray();
        }

        private void AddControl(string option, string currentValue, int colIndex, int rowIndex)
        {
            var optionSplit = option.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (optionSplit.Length < 3
                || !optionSplit[0].Equals("option", StringComparison.OrdinalIgnoreCase)
                || !optionSplit[1].Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            UciConfigValue uciConfigValue = new UciConfigValue {CurrentValue = currentValue};

            var i = 2;
            do
            {
                if (optionSplit[i].Equals("type"))
                {
                    i++;
                    uciConfigValue.OptionType = optionSplit[i];
                    i++;
                    continue;
                }

                if (optionSplit[i].Equals("default"))
                {
                    i++;
                    while (true)
                    {

                        if (i < optionSplit.Length)
                        {
                            if (string.IsNullOrWhiteSpace(uciConfigValue.DefaultValue))
                            {
                                uciConfigValue.DefaultValue = optionSplit[i];
                            }
                            else
                            {
                                uciConfigValue.DefaultValue += " " + optionSplit[i];
                            }
                            i++;
                        }
                        else
                        {
                            break;
                        }

                        if (i < optionSplit.Length)
                        {
                            if (uciConfigValue.OptionType.Equals("combo", StringComparison.OrdinalIgnoreCase))
                            {
                                if (optionSplit[i].Equals("var", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }

                                i++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    continue;
                }

                if (optionSplit[i].Equals("min"))
                {
                    i++;
                    uciConfigValue.MinValue = optionSplit[i];
                    i++;
                    continue;
                }

                if (optionSplit[i].Equals("max"))
                {
                    i++;
                    uciConfigValue.MaxValue = optionSplit[i];
                    i++;
                    continue;
                }
                if (optionSplit[i].Equals("var"))
                {
                    i++;
                    string comboItem = string.Empty;
                    while (true)
                    {

                        if (i < optionSplit.Length)
                        {
                            if (string.IsNullOrWhiteSpace(comboItem))
                            {
                                comboItem = optionSplit[i];

                            }
                            else
                            {
                                comboItem += " " + optionSplit[i];
                            }

                            i++;
                        }
                        else
                        {
                            break;
                        }

                        if (i < optionSplit.Length)
                        {
                            if (optionSplit[i].Equals("var", StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }

                    }

                    uciConfigValue.AddComboItem(comboItem);

                    continue;
                }

                uciConfigValue.OptionName = uciConfigValue.OptionName + optionSplit[i] + " ";
                string firstOrDefault = _uciInfo.OptionValues.FirstOrDefault(o => o.Contains(uciConfigValue.OptionName));
                if (string.IsNullOrWhiteSpace(currentValue))
                {
                    if (!string.IsNullOrWhiteSpace(firstOrDefault))
                    {
                        var strings = firstOrDefault.Split(" ".ToCharArray());
                        uciConfigValue.CurrentValue = strings[strings.Length - 1];
                    }
                    else
                    {
                        uciConfigValue.CurrentValue = string.Empty;
                    }
                }

                i++;
            } while (i < optionSplit.Length);

            switch (uciConfigValue.OptionType)
            {
                case "spin":
                    AddNumericUpDown(uciConfigValue, colIndex, rowIndex);
                    break;
                case "check":
                    AddCheckBox(uciConfigValue, colIndex, rowIndex);
                    break;
                case "combo":
                    AddCombo(uciConfigValue, colIndex, rowIndex);
                    break;
                case "string":
                    AddTextBox(uciConfigValue, colIndex, rowIndex);
                    break;
                case "button":
                    AddButton(uciConfigValue, colIndex, rowIndex);
                    break;
                default:
                    AddUnknown(uciConfigValue, colIndex, rowIndex);
                    break;
            }

        }

        private void AddTextBox(UciConfigValue uciConfigValue, int colIndex, int rowIndex)
        {
            if (rowIndex == gridMain.RowDefinitions.Count)
            {
                gridMain.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });
            }

            var uciTextBoxUserControl = new UciTextBoxUserControl(uciConfigValue);
            var textBlock = new TextBlock()
            {
                Text = uciConfigValue.OptionName,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(textBlock, rowIndex);
            Grid.SetColumn(textBlock, colIndex);
            Grid.SetRow(uciTextBoxUserControl, rowIndex);
            Grid.SetColumn(uciTextBoxUserControl, colIndex + 1);
            gridMain.Children.Add(textBlock);
            gridMain.Children.Add(uciTextBoxUserControl);
        }

        private void AddUnknown(UciConfigValue uciConfigValue, int colIndex, int rowIndex)
        {
            if (rowIndex == gridMain.RowDefinitions.Count)
            {
                gridMain.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });
            }
            var textBlock = new TextBlock()
            {
                Text = uciConfigValue.OptionName,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetRow(textBlock, rowIndex);
            Grid.SetColumn(textBlock, colIndex);
            gridMain.Children.Add(textBlock);
        }

        private void AddCombo(UciConfigValue uciConfigValue, int colIndex, int rowIndex)
        {
            if (rowIndex == gridMain.RowDefinitions.Count)
            {
                gridMain.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });
            }

            var uciComboBoxUserControl = new UciComboBoxUserControl(uciConfigValue);

            var textBlock = new TextBlock()
            {
                Text = uciConfigValue.OptionName,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(textBlock, rowIndex);
            Grid.SetColumn(textBlock, colIndex);
            Grid.SetRow(uciComboBoxUserControl, rowIndex);
            Grid.SetColumn(uciComboBoxUserControl, colIndex + 1);
            gridMain.Children.Add(textBlock);
            gridMain.Children.Add(uciComboBoxUserControl);
        }

        private void AddCheckBox(UciConfigValue uciConfigValue, int colIndex, int rowIndex)
        {
            if (rowIndex == gridMain.RowDefinitions.Count)
            {
                gridMain.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });
            }

            var textBlock = new TextBlock()
            {
                Text = uciConfigValue.OptionName,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var uciCheckBoxUserControl = new UciCheckBoxUserControl(uciConfigValue);
            Grid.SetRow(textBlock, rowIndex);
            Grid.SetColumn(textBlock, colIndex);
            Grid.SetRow(uciCheckBoxUserControl, rowIndex);
            Grid.SetColumn(uciCheckBoxUserControl, colIndex + 1);
            gridMain.Children.Add(textBlock);
            gridMain.Children.Add(uciCheckBoxUserControl);
        }

        private void AddNumericUpDown(UciConfigValue uciConfigValue, int colIndex, int rowIndex)
        {
            if (rowIndex == gridMain.RowDefinitions.Count)
            {
                gridMain.RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });
            }
            var textBlock = new TextBlock
            {
                Text = uciConfigValue.OptionName,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };
            var uciNumericUpDownUserControl = new UciNumericUpDownUserControl(uciConfigValue);

            Grid.SetRow(textBlock, rowIndex);
            Grid.SetColumn(textBlock, colIndex);
            Grid.SetRow(uciNumericUpDownUserControl, rowIndex);
            Grid.SetColumn(uciNumericUpDownUserControl, colIndex + 1);
            gridMain.Children.Add(textBlock);
            gridMain.Children.Add(uciNumericUpDownUserControl);

        }

        private void AddButton(UciConfigValue uciConfigValue, int colIndex, int rowIndex)
        {
            if (rowIndex == gridMain.RowDefinitions.Count)
            {
                gridMain.RowDefinitions.Add(new RowDefinition()
                                            {
                                                Height = GridLength.Auto
                                            });
            }

            var button = new Button()
                         {
                             Content = new TextBlock()
                                       {
                                           Text = uciConfigValue.OptionName,
                                           Margin = new Thickness(1),

                                       },
                             Margin = new Thickness(5),
                             HorizontalAlignment = HorizontalAlignment.Left,
                             VerticalAlignment = VerticalAlignment.Center,
                             Tag = uciConfigValue.OptionName
                         };
            button.Click += btnConfig_Click;
            Grid.SetRow(button, rowIndex);
            Grid.SetColumn(button, colIndex);
            gridMain.Children.Add(button);
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            if (!_showButtons)
            {
                MessageBox.Show(this, "Buttons only work when the engine is loaded", "Not applicable", MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            var s = ((Button) sender).Tag.ToString();
            ButtonConfigEvent?.Invoke(this, new ButtonConfigEventArgs(textBoxName.Text, $"setoption name {s}"));
        }


        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            SaveAsNew = false;
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonDefault_OnClick(object sender, RoutedEventArgs e)
        {
            textBoxName.Text = _uciInfo.Name;
            foreach (UIElement gridMainChild in gridMain.Children)
            {
                if (!(gridMainChild is IUciConfigUserControl)) continue;
                var uciConfigUserControl = gridMainChild as IUciConfigUserControl;
                uciConfigUserControl.ResetToDefault();

            }
        }


        private void ButtonSaveAs_OnClick(object sender, RoutedEventArgs e)
        {
            SaveAsNew = true;
            DialogResult = true;
        }

        private void CheckBoxUseOpeningBook_OnUnchecked(object sender, RoutedEventArgs e)
        {
            radioButtonBest.IsEnabled = false;
            radioButtonFlexible.IsEnabled = false;
            radioButtonWide.IsEnabled = false;
        }

        private void CheckBoxUseOpeningBook_OnChecked(object sender, RoutedEventArgs e)
        {
            radioButtonBest.IsEnabled = true;
            radioButtonFlexible.IsEnabled = true;
            radioButtonWide.IsEnabled = true;
        }
    }
}
