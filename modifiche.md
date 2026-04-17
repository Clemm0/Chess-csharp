Non caricare questo file su git

dotnet run
    compila e runna per provare il progetto

dotnet publish -c Release -r win-x64 --self-contained false
    creare il .exe il più leggero possibile

l'exe si trova a
    bin\Release\net8.0-windows\win-x64\publish\ChessGame.exe