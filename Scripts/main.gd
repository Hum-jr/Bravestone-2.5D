extends Node3D

@onready var fps_label = Label.new()

func _ready():
	# Create a new Label for FPS display
	add_child(fps_label)
	fps_label.position = Vector2(10, 10)
	fps_label.add_theme_color_override("font_color", Color.WHITE)

func _process(delta):
	# Update FPS label every frame
	fps_label.text = "FPS: %d" % Engine.get_frames_per_second()
