   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5   	   J    Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.QuerySelectionWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	   VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>    � SizeToContent$ WidthAndHeight=�6T    � Title$ QuerySelectionWindow��5	   	   $	��450��6&   $	��800��63   .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   � $��0��6    � ColumnDefinitions 5      �� 5      $
��Auto �6#   62   �� 5      $��* �6#   6/   .+�5      �� -   5        $��textBlockSelection��5   (   $��0��6   $��5q�6B   $��0��6M   	Auswahl6d   �� 5      -   5        $��comboBoxSelection��5      $��1��60   6B   5      �� 5   
   -   5        $��buttonOk��5   E   $��1��6   $��5q�6   $
��Left=�6*   $��32��6U   $��32��5      
������ 
������ .��5      �� $$��Assets/Icons/accept_button.png��6   5      �� 5   
   -   5        $��buttonCancel��5   E   $��1��6   $��5q�6   $��Right=�6)   $��32��6Y   $��32��5      
������ 
������ .��5      �� $��Assets/Icons/cancel.png��6   5       5!      5"      