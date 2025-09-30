Была проблема с тем что сайт не грузился нормально + была ошибка стремная. 

Короче, если ты нажал на Shift + Alt + D и выскачила в итоге херовая ошибка
че то про 127.0.0.1:9222/json то нужно просто вызывать killport 9222.


Другая ошибка:
ystem.InvalidOperationException: There is already a subscriber to the content with the given section ID 'mud-overlay-to-popover-provider'.
at Microsoft.AspNetCore.Components.Sections.SectionRegistry.Subscribe(Object identifier, SectionOutlet subscriber)

Как исправить:
Удалить дубликаты:
MudThemeProvider
MudPopoverProvider
MudDialogProvider
MudSnackbarProvider