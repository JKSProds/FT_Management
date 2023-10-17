
/**
* FaceAPI Demo for Browsers
* Loaded via `webcam.html`
*/


// configuration options
const modelPath = '/lib/vladmandic/face-api/model'; // path to model folder that will be loaded using http
const minScore = 0.2; // minimum score
const maxResults = 5; // maximum number of results to return
const recognitionThreshold = 0.5; // Adjust the threshold based on your needs
var detected = 0;
let optionsSSDMobileNet;

// helper function to draw detected faces
function drawFaces(canvas, data, model) {

    var color = 'red';
    const ctx = canvas.getContext('2d', { willReadFrequently: true });
    if (!ctx) return;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    // draw title
    ctx.font = 'small-caps 20px "Segoe UI"';
    for (const person of data) {
        // draw box around each face
        var name = recognizeFace(person.descriptor, model[0], recognitionThreshold).nomeCompleto;
        ctx.lineWidth = 3;
        ctx.strokeStyle = color;
        ctx.fillStyle = color;
        ctx.globalAlpha = 0.6;
        ctx.beginPath();
        ctx.rect(person.detection.box.x, person.detection.box.y, person.detection.box.width, person.detection.box.height);
        ctx.stroke();
        ctx.globalAlpha = 1;
        // draw text labels
        const expression = Object.entries(person.expressions).sort((a, b) => b[1] - a[1]);
        ctx.fillStyle = 'black';
        ctx.fillText(`Nome: ${name}`, person.detection.box.x, person.detection.box.y - 77);
        ctx.fillText(`gender: ${Math.round(100 * person.genderProbability)}% ${person.gender}`, person.detection.box.x, person.detection.box.y - 59);
        ctx.fillText(`expression: ${Math.round(100 * expression[0][1])}% ${expression[0][0]}`, person.detection.box.x, person.detection.box.y - 41);
        ctx.fillText(`age: ${Math.round(person.age)} years`, person.detection.box.x, person.detection.box.y - 23);
        ctx.fillText(`roll:${person.angle.roll}° pitch:${person.angle.pitch}° yaw:${person.angle.yaw}°`, person.detection.box.x, person.detection.box.y - 5);
        ctx.fillStyle = 'lightblue';

        ctx.fillText(`Nome: ${name}`, person.detection.box.x, person.detection.box.y - 78);
        ctx.fillText(`gender: ${Math.round(100 * person.genderProbability)}% ${person.gender}`, person.detection.box.x, person.detection.box.y - 60);
        ctx.fillText(`expression: ${Math.round(100 * expression[0][1])}% ${expression[0][0]}`, person.detection.box.x, person.detection.box.y - 42);
        ctx.fillText(`age: ${Math.round(person.age)} years`, person.detection.box.x, person.detection.box.y - 24);
        ctx.fillText(`roll:${person.angle.roll}° pitch:${person.angle.pitch}° yaw:${person.angle.yaw}°`, person.detection.box.x, person.detection.box.y - 6);
        // draw face points for each face
        ctx.globalAlpha = 0.8;
        ctx.fillStyle = 'lightblue';
        const pointSize = 2;
        for (let i = 0; i < person.landmarks.positions.length; i++) {
            ctx.beginPath();
            ctx.arc(person.landmarks.positions[i].x, person.landmarks.positions[i].y, pointSize, 0, 2 * Math.PI);
            ctx.fill();
        }
    }
}

function recognizeFace(detectedDescriptor, model, threshold) {
    for (let i = 0; i < model.length; i++) {
        if (model[i].faceRec.split(' ').length == detectedDescriptor.length) {
            const distance = faceapi.euclideanDistance(detectedDescriptor, model[i].faceRec.split(' '));

            if (distance < threshold) {
                // Face recognized as a match
                return model[i];
            }
        }
    }

    // Face not recognized
    return 'UNKNOWN';
}

async function detectVideo(video, canvas) {
    if (!video || video.paused) return false;
    var model = [
        @Html.Raw(Json.Serialize(Model.ToList()))
    ];
    const t0 = performance.now();
    faceapi
        .detectAllFaces(video, optionsSSDMobileNet)
        .withFaceLandmarks()
        .withFaceExpressions()
        .withFaceDescriptors()
        .withAgeAndGender()
        .then((result) => {
            drawFaces(canvas, result, model);
            for (const person of result) {
                if (recognizeFace(person.descriptor, model[0], recognitionThreshold).id == document.getElementById('txtUtilizadorSelecionado').value) detected++;
                if (detected > 20) {
                    RegistarAcesso();
                    detected = 0;
                }
            }
            requestAnimationFrame(() => detectVideo(video, canvas));
            return true;
        })
    return false;
}

// just initialize everything and call main function
async function setupCamera() {
    const video = document.getElementById('video');
    const canvas = document.getElementById('canvas');
    if (!video || !canvas) return null;


    let stream;
    const constraints = { audio: false, video: { facingMode: 'user', resizeMode: 'crop-and-scale' } };
    if (window.innerWidth > window.innerHeight) constraints.video.width = { ideal: window.innerWidth };
    else constraints.video.height = { ideal: window.innerHeight };
    stream = await navigator.mediaDevices.getUserMedia(constraints);
    if (stream) {
        video.srcObject = stream;
    } else {
        return null;
    }
    const track = stream.getVideoTracks()[0];
    const settings = track.getSettings();
    if (settings.deviceId) delete settings.deviceId;
    if (settings.groupId) delete settings.groupId;
    if (settings.aspectRatio) settings.aspectRatio = Math.trunc(100 * settings.aspectRatio) / 100;
    return new Promise((resolve) => {
        video.onloadeddata = async () => {
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            video.play();
            detectVideo(video, canvas);
            resolve(true);
        };
    });
}

async function setupFaceAPI() {
    // load face-api models
    // log('Models loading');
    // await faceapi.nets.tinyFaceDetector.load(modelPath); // using ssdMobilenetv1
    await faceapi.nets.ssdMobilenetv1.load(modelPath);
    await faceapi.nets.ageGenderNet.load(modelPath);
    await faceapi.nets.faceLandmark68Net.load(modelPath);
    await faceapi.nets.faceRecognitionNet.load(modelPath);
    await faceapi.nets.faceExpressionNet.load(modelPath);
    optionsSSDMobileNet = new faceapi.SsdMobilenetv1Options({ minConfidence: minScore, maxResults });
}

async function main() {

    // default is webgl backend
    await faceapi.tf.setBackend('webgl');
    await faceapi.tf.ready();

    // tfjs optimizations
    if (faceapi.tf?.env().flagRegistry.CANVAS2D_WILL_READ_FREQUENTLY) faceapi.tf.env().set('CANVAS2D_WILL_READ_FREQUENTLY', true);
    if (faceapi.tf?.env().flagRegistry.WEBGL_EXP_CONV) faceapi.tf.env().set('WEBGL_EXP_CONV', true);
    if (faceapi.tf?.env().flagRegistry.WEBGL_EXP_CONV) faceapi.tf.env().set('WEBGL_EXP_CONV', true);

    await setupFaceAPI();
    await setupCamera();
}

// start processing as soon as page is loaded
window.onload = main;