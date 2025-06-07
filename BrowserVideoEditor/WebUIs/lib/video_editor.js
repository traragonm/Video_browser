let socket;

const videoElement = $("#videoContainer")

$(function () {

    initSocket();



    $('#ProcessBtn').click(function () {

        // addFile();
        let url = $("#videoUrlTextarea").val()
        let path = $("#savePathField").val()

        let duration = $("#durationField").val()
        let space = $("#spaceField").val()

        let data = {
            urls: url,
            saveFolderPath: path,
            splitDuration: duration,
            splitSpace: space
        };

        if (socket && socket.readyState === WebSocket.OPEN) {
            socket.send(JSON.stringify(data));
        } else {
            console.log("Socket is not connected.")
        }

    });
    $('#UpdateBtn').click(function () {
        updateFileStatus();
    })

});


const initSocket = () => {


    socket = new WebSocket('ws://localhost:5843/videoEdit'); // or wss:// for secure



    socket.onopen = function () {
        console.log('WebSocket Connected');

        // $('#messages').append('<p>Connected</p>');
    };

    socket.onmessage = function (event) {
        let res = JSON.parse(event.data);

        switch (res.code) {
            case 1:// in queue
                addFile(res)
                break;
            case 2://ERROR
                updateErrorFileStatus(res)
                break;
            case 3://PROCESSING
            case 4://DOWNLOADING
            case 5: //DOWNLOADED
            case 6://PROCESSED
                updateProcessFileStatus(res)
                break;
        }


        // $('#messages').append('<p>Received: ' + event.data + '</p>');
    };

    socket.onclose = function () {
        // $('#messages').append('<p>Disconnected</p>');
        console.log('WebSocket closed');

    };

    socket.onerror = function (error) {
        // $('#messages').append('<p>Error: ' + error.message + '</p>');
        console.log('WebSocket error' + error.message);

        socket.onopen = null;
        socket.onmessage = null;
        socket.onclose = null;
        socket.onerror = null;

        console.error('WebSocket error:', error);
    };
}

const addFile = (data) => {

    let item = '<tr id="' + data.data.id +
        '" class="file-row">' +
        '<td><div class="file_name">' + data.data.title + '</div></td>' +
        '<td><div class="file_path"></div></td>' +
        '<td><div class="file_extension">' + data.data.extension + '</div></td>' +
        '<td><div class="file_status"><span class="badge bg-primary">in queue</span></div></td></tr>';
    $(videoElement).find('tbody').append(item);
}
const updateErrorFileStatus = (data) => {
    // let itemStatus = '<span class="badge bg-success">done</span>';
    // $(videoElement).find('tbody tr#1 .file-status').html('')
    // $(videoElement).find('tbody tr#1 .file-status').append(itemStatus)

}

const updateProcessFileStatus = (data) => {

    if (data.code == 3) {
        let itemStatus = '<span class="badge bg-secondary">done</span>';
        $(videoElement).find('tbody tr#' + data.data.id + ' .file_status').html('')
        $(videoElement).find('tbody tr#' + data.data.id + ' .file_status').append(itemStatus)
    }
    if (data.code == 4) {
        let itemStatus = '<span class="badge bg-warning">done</span>';
        $(videoElement).find('tbody tr#' + data.data.id + ' .file_status').html('')
        $(videoElement).find('tbody tr#' + data.data.id + ' .file_status').append(itemStatus)
    }
    if (data.code == 5) {
        let itemStatus = '<span class="badge bg-info">done</span>';
        $(videoElement).find('tbody tr#' + data.data.id + ' .file_status').html('')
        $(videoElement).find('tbody tr#' + data.data.id + ' .file_status').append(itemStatus)
    }
    if (data.code == 6) {
        let itemStatus = '<span class="badge bg-success">done</span>';
        $(videoElement).find('tbody tr#' + data.data.id + ' .file_status').html('')
        $(videoElement).find('tbody tr#' + data.data.id + ' .file_status').append(itemStatus)
        let itemPath = '<span>' + data.data.path + '</span>';

        $(videoElement).find('tbody tr#' + data.data.id + ' .file_path').append(itemPath)


    }



}

