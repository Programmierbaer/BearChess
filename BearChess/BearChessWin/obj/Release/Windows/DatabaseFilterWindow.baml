   M S B A M L   `   `   `  ����   BearChessWin:    4www.SoLaNoSoft.com.BearChessWin.DatabaseFilterWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     � SizeToContent$  WidthAndHeight=� � Icon$&  ../Assets/Icons/table_filter.png�� � 
ResizeMode$ NoResize=� � Title$ Database Filter��$	��430��$	��450��.���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� -   $��checkBoxEnableFilter��$��5q�$��0��.��Enable filter� -   $��
gridFilter��$��1��$��5q�
������� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��5q�$��0��$��0��.+��� -   $��checkBoxTournament��$��5q�$��No tournament games���� -   $��checkBoxDuel��$��5q�$��No duel games��� $��5q�$��1��$��0�� � ColumnDefinitions �� $
��Auto ��� $��* � �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��5q�$��0��$��0��	Player:�� -   $��textBoxWhite��$��5q�$��1��$��0���� $��1��$��1�� �� Orientation$ 
Horizontal=�.+�	� -   $��radioButtonWhite�� 	� 	GroupName$ Player1��$��5q� d� 	IsChecked$
 TrueY�.��White	� -   $��radioButtonWhiteWhatever��$ Player1��$��5q�.��
Whatever�� $��5q�$��0��$��2��	Player:�� -	   $��textBoxBlack��$��5q�$��1��$��2���� $��3��$��1��$ 
Horizontal=�.+�	� -
   $��radioButtonBlack��$ Player2��$��5q�$
 TrueY�.��Black	� -   $��radioButtonBlackWhatever��$ Player2��$��5q�.��
Whatever�� $��5q�$��0��$��4��Event:�� -   $��textBoxEvent��$��5q�$��1��$��4���� $��5q�$��0��$��2��$ 
Horizontal=�.+��� $��5q�$��Center=�From:(  "System.Windows.Controls.DatePicker  -   $��datePickerFromDate��$��Center=�$��10,5,5,5q��� $��5q�$��Center=�To:  -   $��datePickerToDate��$��Center=�$��5q��� $��5q�$��0��$��3��$ 
Horizontal=�.+��� -   $��buttonApplyFilter��$��5q�
������ 
������ 	 � ToolTip$	 Apply filter settings��.���� $(��"../Assets/Icons/filter_reapply.png��$��32��$��32���� -   $��buttonClearFilter��$��15,5,5,5q�
������ 
������ $	 Clear filter settings��.���� $&�� ../Assets/Icons/filter_clear.png��$��32��$��32��]� $��2����.  .+��� -   $��
buttonSave��$��10q�$
��Left=�
������ 
������ $	 Save filter as default��.���� $"��../Assets/Icons/diskette.png��$��32��$��32���� -   $��buttonCancel��$��10q�$��Right=�
������ 
������ $	 Close��.���� $"��../Assets/Icons/door_out.png��$��32��$��32��