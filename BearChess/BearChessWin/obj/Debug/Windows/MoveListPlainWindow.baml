   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   9    3www.SoLaNoSoft.com.BearChessWin.MoveListPlainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	   -   5        :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources    Moves � Title# Z  5   	   $	��450��6.   $	��800��6;    J� CommandBindings 5	      )  #System.Windows.Input.CommandBinding  5
   
   -   5           Command$
 Copy��5
      6s   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   � $��5q�6   $��0��6   
������� 5      �� 5      $
��Auto �6    60   �� 5      $
��Auto �6    60   �� 5      $
��Auto �6    60    � ColumnDefinitions 5      �� 5      $
��Auto �6#   62   �� 5      $
��Auto �6#   62   .+�5      �� -   5        $��textBlockResult��5   9   $��5q�6   $��DemiBold"�6#   $��0��6P   $��2��6`   $��2��6u   *6�   �� 5      -   5        $��textBlockWhite��5      $��Center=�6.   $��5q�6I   $��0��6T   $��0��6a   K6t   �� 5      -   5        $��textBlockWhitePlayer��5   F   $��2,5,5,5q�6   $��0��6)   $��1��66   6d   �� 5       -   5        $��textBlockBlack��5       $��Center=�6.   $��5,5,5,0q�6J   $��1��6[   $��0��6h   k6{   �� 5!      -   5        $��textBlockBlackPlayer��5!   F   $��2,5,5,0q�6   $��1��6)   $��1��66   6d   5"      �� 5$   
   -   5        $��scrollViewer��5$      $��1��6+   $��Disabled=�68   .��5%      �� -	   5        $��stackPanelMoves��5%      60   5&      ]� 5'   
   $��5q�6   $��2��6   ��.  .+�5)      �� -
   5        $��buttonExtend��5)       $��5q�6   $��32��6Q   $��32��6\   
������ 
������ $
��Left=�5*         ChangeContent � ToolTip# Z 60   .��5+      �� $%��../Assets/Icons/script_gear.png��6   5,      �� 5-      -   5        $��buttonShowHideComments��5-       $��5q�6   $��32��6e   $��32��6p   
������ 
������ $
��Left=�5.         ShowHideComments# Z 60   .��5/      �� $"��../Assets/Icons/comments.png��6   50      �� 51      -   5        $��buttonShowHideBuddy��51       $��5q�6   $��32��6_   $��32��6j   
������ 
������ $
��Left=�52      	   ShowHideBuddyBestLines# Z	 60   .��53      �� $&�� ../Assets/Icons/user_comment.png��6   54      �� 55      -   5        $��buttonFontInc��55       $��5q�6   $��32��6S   $��32��6^   
������ 
������ $
��Left=�56      
   IncreaseFontSize# Z
 60   .��57      �� $,��&../Assets/Icons/font_size_decrease.png��6   58      �� 59      -   5        $��buttonFontDec��59       $��5q�6   $��26��6S   $��26��6^   
������ 
������ $
��Left=�5:         DecreaseFontSize# Z 60   .��5;      �� $,��&../Assets/Icons/font_size_increase.png��6   5<      �� 5=      -   5        $��
buttonCopy��5=   =   $��15,5,5,5q�6   $
��Left=�6'   $
��Copy��6Q   
������ 
������    CopyToClipboard# Z 6�   .��5>      �� $,��&../Assets/Icons/clipboard_sign_out.png��6   $��32��6H   $��32��6S   6a   5?      5A      �� 5B   
   -   5         ��Name$ textBlockContent��5B      $��5q�6.   $��3��69   Content: Only moves6\   5C      5E      