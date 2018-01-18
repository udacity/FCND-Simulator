var LibraryGetHostname = {
  ObtainHost: function() {
    var fullHost = document.location.hostname;
    if (fullHost === "") {
      console.log("no hostname");
      return 0;
    }
    console.log(fullHost);

    var parts = fullHost.split(".");
    var host = parts[0].split("-");

    var socketLoc = "ws://127.0.0.1:3001";
    if (fullHost == "classroom.udacity.com") {
      var socketLoc = "wss://" + host[0] + "-3001." + parts[1] + "." + parts[2];
    }
    console.log("WebSocket location ", socketLoc);

    var bufferSize = lengthBytesUTF8(socketLoc) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(socketLoc, buffer, bufferSize);

    return buffer;
  }
};

mergeInto(LibraryManager.library, LibraryGetHostname);
