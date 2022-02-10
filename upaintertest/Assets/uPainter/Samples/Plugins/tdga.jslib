mergeInto(LibraryManager.library, {
	postTDGAEvent: function (eventid, name) {
        var TDGA = window.TDGA;
        eventid = Pointer_stringify(eventid);
        name = Pointer_stringify(name);

		if(TDGA) {
            TDGA.onEvent (eventid, {
                type: name
            });
        }
        console.log("event=" + eventid + ",type=" + name);
	},
 });