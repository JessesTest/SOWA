(function () {
    var appPath = null;
    function getAppPath() {
        if (appPath === null) {
            var temp = $("script").filter("[src*='/js/snco.']")[0].src;
            var index = temp.indexOf("/js/snco.");
            appPath = temp.substring(0, index);
        }
        return appPath;
    }
    window.getPath = function (inPath) {
        if (typeof inPath == "string" && inPath.length > 0 && inPath.charAt(0) == "~")
            return getAppPath() + inPath.substring(1);
        return inPath;
    };
})();
