   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5   	   D    >www.SoLaNoSoft.com.BearChessWpfCustomControlLib.ProgressWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5   	     � SizeToContent$  Height=�5       � WindowStyle$ 
ToolWindow=�61    � WindowStartupLocation$ CenterOwner=�6J    � Title$ Information��5   	   $	��270��6   $	��300��6*   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $��* �6    � RowDefinitions 5   
   �� 5      $
��Auto �6   6-   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   �� -   5        $��textBlockWait��5      $��20q�6)   $��0��65   $��0��6E   $��Center=�6R   $��DemiBold"�6o   $��Center=�6�   Please wait...6�   � 5   
   -   5        $��progressBar��5      $��20q�6)   $��0��65   $��1��6E   $��Center=�6R   $��Center=�6o   $	��200��6�   $��20��6�   6�   �� 5   
   -   5        $��textBlockInfo��5      $��20q�6)   $��0��65   $��2��6E   $��Center=�6R   $��Center=�6o   $	��200��6�    �� TextAlignment$ Center=�6�   6�   �� 5   
   -   5        $��buttonCancel��5   [   $��5,5,5,5q�6   $��Right=�6"   $��0��6>   $��3��6N   
������ 
������ .��5      �� $��Assets/Icons/cancel.png��6   $��32��65   $��32��6@   5      5      5      