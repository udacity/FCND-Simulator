var LibraryGetHostname = {
  ObtainHost: function() {
    var fullHost = document.location.hostname;
    if (fullHost === "") {
      console.log("no hostname");
      return 0;
    }

    var parts = fullHost.split(".");
    var host = parts[0].split("-");
    var socketLoc = "wss://" + host[0] + "-####." + parts[1] + "." + parts[2];
    // for local dev debugging
    // var socketLoc = "ws://127.0.0.1:5760"
    console.log("WebSocket location ", socketLoc);

    var bufferSize = lengthBytesUTF8(socketLoc) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(socketLoc, buffer, bufferSize);

    return buffer;
  }
};

mergeInto(LibraryManager.library, LibraryGetHostname);
