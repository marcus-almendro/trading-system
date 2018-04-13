// Crockford's supplant method (poor man's templating)
if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}

var templateBook = {
    buy:
        `<tr data-id="{Id}">
            <td>{Id}</td>
            <td>{Size}</td>
            <td>{Price}</td>
        </tr>`,
    sell:
        `<tr data-id="{Id}">
            <td>{Price}</td>
            <td>{Size}</td>
            <td>{Id}</td>
        </tr>`
};

var templateTrade =
    `<tr>
    <td>{Id}</td>
    <td>{TakerSide}</td>
    <td>{Price}</td>
    <td>{ExecutedSize}</td>
</tr>`;

syncing = true;
messagesWhileSyncing = [];
allMessages = {};

function handleMessage(m) {
    if (syncing) {
        handleMessageWhileSyncing(m);
    } else {
        handleLiveMessage(m);
    }
}

function handleMessageWhileSyncing(m) {
    messagesWhileSyncing.push(m);
}

function endSyncing(snapshotMessages) {
    console.log('processing snapshot messages, total:' + snapshotMessages.length);
    for (let i = 0; i < snapshotMessages.length; i++) {
        const m = snapshotMessages[i];
        allMessages[m.Id] = m;
        handleLiveMessage(m);
    }

    console.log('processing messages received while syncing');
    for (let i = 0; i < messagesWhileSyncing.length; i++) {
        const m = messagesWhileSyncing[i];
        if (allMessages[m.Id]) {
            console.log('ignoring duplicated message');
        }
        handleLiveMessage(m);
    }

    syncing = false;
    allMessages = null;
    messagesWhileSyncing = null;
}

var orderFuncs = {
    1: newOrder, 2: updOrder, 3: delOrder
};

function handleLiveMessage(m) {
    //console.log(m);
    if (m.Type === 0) {
        orderFuncs[m.Order.Action](m.Order);
    }

    if (m.Type === 1) {
        newTrade(m.Trade);
    }
}

function newTrade(t) {
    //console.log('adding trade');
    t.TakerSide = side(t.TakerSide);
    $('#tradesTable tbody').prepend(templateTrade.supplant(t));
}

function side(s) {
    return s == 0 ? 'buy' : 'sell';
}

function tableBody(s) {
    return '#' + side(s) + 'Table tbody';
}

function newOrder(o) {
    //console.log('adding order ' + o.Id)

    if (o.Position === 0) {
        $(tableBody(o.Side)).prepend(templateBook[side(o.Side)].supplant(o));
        return;
    }

    $(tableBody(o.Side) + " > tr:nth-child(" + (o.Position) + ")").after(templateBook[side(o.Side)].supplant(o));
}

function updOrder(o) {
    //console.log('updating order ' + o.Id)
    var tr = $(tableBody(o.Side) + ' tr[data-id=' + o.Id + ']');
    if (tr.length > 0) {
        tr.html($(templateBook[side(o.Side)].supplant(o)).html());
    }
}

function delOrder(o) {
    //console.log('deleting order ' + o.Id)
    $(tableBody(o.Side) + ' tr[data-id=' + o.Id + ']').remove();
}

var socket = io();
socket.on('msg', handleMessage);
socket.on('snp_msg', endSyncing);

setTimeout(function () {
    console.log('getting snapshot');
    socket.emit('snapshot request');
}, 500)

var errorCodes = {
    1: 'Success',
    2: 'AlreadyExists',
    3: 'InvalidSymbol',
    4: 'InvalidOrderId',
    5: 'InvalidArgument',
    6: 'Timeout',
};

socket.on('API_ERR', function (res) {
    console.log(res);
});
socket.on('API_OK', function (res) {
    console.log(errorCodes[res.Value]);
});

$('form').submit(function (e) {
    e.preventDefault();
    var action = 1;
    var orderId = $('#orderId').val() * 1;
    var size = $('#size').val() * 1;

    if (orderId) {
        action = 2;
        if (size === 0) {
            action = 3;
        }
    } else {
        orderId = new Date().valueOf();
    }

    var order = {
        Action: action,
        Symbol: '',
        Id: orderId,
        IsBuySide: $('#side').val() === "1",
        Price: $('#price').val() * 1,
        Size: size,
        UserId: 0,
        Position: 0
    };
    socket.emit('new order', order);
});