   M S B A M L   `   `   `  ����   BearChessWinH    Bwww.SoLaNoSoft.com.BearChessWin.UserControls.UciLogInfoUserControl   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     .���   � RowDefinitions  �� $
��Auto ��� $��* ��� $
��Auto �.+��� -   $��textBlockName��$��0��$
��Left=�$��Center=�$��5q��� -   $��listBoxInfo��$��5q�$��1���� $��5q�$��2�� �� Orientation$ 
Horizontal=�.+��� -   $��
buttonStop��$��Center=�$��5,0,5,0q�$��24��$��24��
������ 
������  � ToolTip$ Stop/Go��.���� .+��� -   $��	imagePlay��$+��%../Assets/Icons/control_play_blue.png��$y�	Collapsed=��� -   $��
imagePause��$,��&../Assets/Icons/control_pause_blue.png���� -   $��buttonClear��$��Center=�$��0,0,5,0q�$��24��$��24��
������ 
������ $ Clear��.���� $*��$../Assets/Icons/clear_formatting.png���� -   $��buttonClose��$��Center=�$��0,0,5,0q�$��24��$��24��
������ 
������ $ Close��.���� $"��../Assets/Icons/door_out.png���� $��Center=�$��5,0,5,0q�
Command:�� -   $��textBoxCommand��$��0,0,5,0q�$	��200��$ Enter an UCI command.���� -	   $��
buttonSend��$��Center=�$��0,0,5,0q�$��24��$��24��
������ 
������ $ Send command to engine��.���� $#��../Assets/Icons/bullet_go.png���� -
   $��buttonClipboard��$��Center=�$��0,0,5,0q�$��24��$��24��
������ 
������ $ Copy to clipboard��.���� $(��"../Assets/Icons/clipboard_sign.png��