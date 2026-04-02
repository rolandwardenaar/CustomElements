/**
 * Protobuf schema definitions matching protos/drawing.proto.
 *
 * Uses protobufjs programmatic API so no `protoc` code generation is needed.
 * If the proto file changes, update the types and schema here accordingly.
 */
import protobuf from 'protobufjs/light';

// ── Namespace ──────────────────────────────────────────────────
const root = new protobuf.Root();
const drawingNs = root.define('drawing');

// ── Simple messages ────────────────────────────────────────────
const Empty = new protobuf.Type('Empty');
drawingNs.add(Empty);

const SceneRequest = new protobuf.Type('SceneRequest')
  .add(new protobuf.Field('sceneName', 1, 'string'));
drawingNs.add(SceneRequest);

const SceneList = new protobuf.Type('SceneList')
  .add(new protobuf.Field('sceneNames', 1, 'string', 'repeated'));
drawingNs.add(SceneList);

// ── Draw-command payload messages ──────────────────────────────
const SetColor = new protobuf.Type('SetColor')
  .add(new protobuf.Field('color', 1, 'string'));
drawingNs.add(SetColor);

const SetStrokeColor = new protobuf.Type('SetStrokeColor')
  .add(new protobuf.Field('color', 1, 'string'));
drawingNs.add(SetStrokeColor);

const MoveTo = new protobuf.Type('MoveTo')
  .add(new protobuf.Field('x', 1, 'double'))
  .add(new protobuf.Field('y', 2, 'double'));
drawingNs.add(MoveTo);

const LineTo = new protobuf.Type('LineTo')
  .add(new protobuf.Field('x', 1, 'double'))
  .add(new protobuf.Field('y', 2, 'double'));
drawingNs.add(LineTo);

const FillRect = new protobuf.Type('FillRect')
  .add(new protobuf.Field('x', 1, 'double'))
  .add(new protobuf.Field('y', 2, 'double'))
  .add(new protobuf.Field('width', 3, 'double'))
  .add(new protobuf.Field('height', 4, 'double'));
drawingNs.add(FillRect);

const Arc = new protobuf.Type('Arc')
  .add(new protobuf.Field('x', 1, 'double'))
  .add(new protobuf.Field('y', 2, 'double'))
  .add(new protobuf.Field('radius', 3, 'double'))
  .add(new protobuf.Field('startAngle', 4, 'double'))
  .add(new protobuf.Field('endAngle', 5, 'double'));
drawingNs.add(Arc);

const BeginPath = new protobuf.Type('BeginPath');
drawingNs.add(BeginPath);

const ClosePath = new protobuf.Type('ClosePath');
drawingNs.add(ClosePath);

const Fill = new protobuf.Type('Fill');
drawingNs.add(Fill);

const Stroke = new protobuf.Type('Stroke');
drawingNs.add(Stroke);

const SetLineWidth = new protobuf.Type('SetLineWidth')
  .add(new protobuf.Field('width', 1, 'double'));
drawingNs.add(SetLineWidth);

const ClearCanvas = new protobuf.Type('ClearCanvas')
  .add(new protobuf.Field('width', 1, 'double'))
  .add(new protobuf.Field('height', 2, 'double'));
drawingNs.add(ClearCanvas);

const QuadraticCurveTo = new protobuf.Type('QuadraticCurveTo')
  .add(new protobuf.Field('cpx', 1, 'double'))
  .add(new protobuf.Field('cpy', 2, 'double'))
  .add(new protobuf.Field('x', 3, 'double'))
  .add(new protobuf.Field('y', 4, 'double'));
drawingNs.add(QuadraticCurveTo);

const BezierCurveTo = new protobuf.Type('BezierCurveTo')
  .add(new protobuf.Field('cp1x', 1, 'double'))
  .add(new protobuf.Field('cp1y', 2, 'double'))
  .add(new protobuf.Field('cp2x', 3, 'double'))
  .add(new protobuf.Field('cp2y', 4, 'double'))
  .add(new protobuf.Field('x', 5, 'double'))
  .add(new protobuf.Field('y', 6, 'double'));
drawingNs.add(BezierCurveTo);

// ── DrawCommand (oneof) ────────────────────────────────────────
const DrawCommand = new protobuf.Type('DrawCommand')
  .add(new protobuf.OneOf('command', [
    'setColor', 'moveTo', 'lineTo', 'fillRect', 'arc',
    'beginPath', 'closePath', 'fill', 'stroke', 'setLineWidth',
    'clearCanvas', 'quadraticCurveTo', 'bezierCurveTo', 'setStrokeColor',
  ]))
  .add(new protobuf.Field('setColor', 1, 'SetColor'))
  .add(new protobuf.Field('moveTo', 2, 'MoveTo'))
  .add(new protobuf.Field('lineTo', 3, 'LineTo'))
  .add(new protobuf.Field('fillRect', 4, 'FillRect'))
  .add(new protobuf.Field('arc', 5, 'Arc'))
  .add(new protobuf.Field('beginPath', 6, 'BeginPath'))
  .add(new protobuf.Field('closePath', 7, 'ClosePath'))
  .add(new protobuf.Field('fill', 8, 'Fill'))
  .add(new protobuf.Field('stroke', 9, 'Stroke'))
  .add(new protobuf.Field('setLineWidth', 10, 'SetLineWidth'))
  .add(new protobuf.Field('clearCanvas', 11, 'ClearCanvas'))
  .add(new protobuf.Field('quadraticCurveTo', 12, 'QuadraticCurveTo'))
  .add(new protobuf.Field('bezierCurveTo', 13, 'BezierCurveTo'))
  .add(new protobuf.Field('setStrokeColor', 14, 'SetStrokeColor'));
drawingNs.add(DrawCommand);

// ── Exports ────────────────────────────────────────────────────
export {
  root,
  Empty,
  SceneRequest,
  SceneList,
  DrawCommand,
  SetColor,
  SetStrokeColor,
  MoveTo,
  LineTo,
  FillRect,
  Arc,
  BeginPath,
  ClosePath,
  Fill,
  Stroke,
  SetLineWidth,
  ClearCanvas,
  QuadraticCurveTo,
  BezierCurveTo,
};

// ── TypeScript helper interfaces ───────────────────────────────

/** Decoded DrawCommand with exactly one command field set. */
export interface DecodedDrawCommand {
  command: string; // oneof field name, e.g. 'setColor'
  setColor?: { color: string };
  setStrokeColor?: { color: string };
  moveTo?: { x: number; y: number };
  lineTo?: { x: number; y: number };
  fillRect?: { x: number; y: number; width: number; height: number };
  arc?: { x: number; y: number; radius: number; startAngle: number; endAngle: number };
  beginPath?: Record<string, never>;
  closePath?: Record<string, never>;
  fill?: Record<string, never>;
  stroke?: Record<string, never>;
  setLineWidth?: { width: number };
  clearCanvas?: { width: number; height: number };
  quadraticCurveTo?: { cpx: number; cpy: number; x: number; y: number };
  bezierCurveTo?: { cp1x: number; cp1y: number; cp2x: number; cp2y: number; x: number; y: number };
}
