/**
Copyright 2014-2017 Robert McNeel and Associates

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
**/

#pragma once

#include <vector>
#include <chrono>
#include <ctime>
#include <thread>
#include <mutex>
#include <string>

#pragma warning ( push )

#pragma warning ( disable : 4244 )

#include "background.h"
#include "camera.h"
#include "colorspace.h"
#include "device.h"
#include "film.h"
#include "graph.h"
#include "integrator.h"
#include "light.h"
#include "mesh.h"
#include "nodes.h"
#include "object.h"
#include "scene.h"
#include "session.h"
#include "shader.h"

#include "util_color.h"
#include "util_function.h"
#include "util_progress.h"
#include "util_string.h"
#include "util_thread.h"

#pragma warning ( pop )

#include "ccycles.h"

#define MULTIDEVICEOFFSET 100000
#define ISMULTIDEVICE(id) (id>=MULTIDEVICEOFFSET)
#define MULTIDEVICEIDX(id) (id-MULTIDEVICEOFFSET)
#define GETDEVICE(puthere, id) \
	if (ISMULTIDEVICE(id)) { \
		(puthere) = multi_devices[MULTIDEVICEIDX(id)]; \
	} \
	else { \
		(puthere) = devices[id]; \
	} 

//extern LOGGER_FUNC_CB logger_func;
extern std::vector<LOGGER_FUNC_CB> loggers;

extern std::ostream& operator<<(std::ostream& out, shadernode_type const &snt);

/* Simple class to help with debug logging. */
class Logger {
public:
	bool tostdout{ false };

	/* Variadic template function so we can handle
	 * any amount of arguments. Each subsequent call
	 * adds the head to logger_msg.
	 */
	template<typename T, typename... Tail>
	void logit(unsigned int client_id, T head, Tail... tail) {
		m.lock();
		/* reset logger_msg */
		logger_msg.str("");

		/* get current time as string. std::ctime returns char* that
		 * has \n at the end. Make sure we don't have that, otherwise
		 * our beautiful output gets broken up.
		 */
		auto t = std::chrono::system_clock::now();
		std::time_t ts = std::chrono::system_clock::to_time_t(t);
		auto tsstr = std::string{std::ctime(&ts)};
		tsstr = tsstr.substr(0, tsstr.size() - 1);

		/* start our new logger_msg with timestamp and head, then
		 * follow up :)
		 */
		logger_msg << tsstr << ": " << head;
		logit_followup(client_id, tail...);
	}

private:
	template<typename T, typename... Tail>
	void logit_followup(unsigned int client_id, T head, Tail... tail) {
		logger_msg << head;
		logit_followup(client_id, tail...);
	}

	/* The final logit function call that will realise the
	 * stringstream constructed by the variadic template
	 * function. Upon completion reset the logger_msg to
	 * an empty string.
	 */
	void logit_followup(unsigned int client_id) {
#if defined(DEBUG)
		LOGGER_FUNC_CB logger_func = loggers[client_id];
		if (logger_func) logger_func(logger_msg.str().c_str());

		// also print to std::cout if wanted
		if (tostdout) std::cout << logger_msg.str().c_str() << std::endl;
#endif
		m.unlock();
	}

	std::stringstream logger_msg;

	std::mutex m;
};

/*
 * The logger facility to use.
 *
 * Usage:
 * logger.logit("This is a message", var, var2, " and some more", var3);
 */
extern Logger logger;

struct CCImage {
		std::string filename;
		void *builtin_data;

		int width;
		int height;
		int depth;
		int channels;
		bool is_float;
};

#ifndef GLuint
typedef unsigned int GLuint;
#endif

class CCSession final {
public:
	unsigned int id{ 0 };
	ccl::SessionParams params;
	ccl::Session* session = nullptr;

	GLuint program = 0;

	/* The status update handler for ccl::Session update callback.
	 */
	void status_update(void);
	/* The test cancel handler for ccl::Session cancellation test
	 */
	void test_cancel(void);
	/* The update render tile handler for ccl::Session rendertile update callback.
	 */
	void update_render_tile(ccl::RenderTile &tile, bool highlight);
	/* The write render tile handler for ccl::Session rendertile write callback.
	 */
	void write_render_tile(ccl::RenderTile &tile);
	/* The display update handler for ccl::Session.
	 *
	 */
	void display_update(int sample);

	/* Hold the pixel buffer with the final result for the attached session.
	 * Gets updated by update_render_tile and write_render_tile.
	 */
	float* pixels = nullptr;
	unsigned int buffer_size{ 0 };
	unsigned int buffer_stride{ 0 }; // number of float values for one pixel

	int width{ 0 };
	int height{ 0 };

	ccl::BufferParams buffer_params;

	/* Create a new CCSession, initialise all necessary memory. */
	static CCSession* create(int width, int height, unsigned int buffer_stride);

	/* When a session is reset we need to recreate the pixel buffer based on new
	 * info.
	 */
	void reset(int width, int height, unsigned int buffer_stride_);

	/* Returns true if size was changed. Will reset the has_changed flag. */
	bool size_has_changed();
	ccl::thread_mutex pixels_mutex;

	/*~CCSession() {
		ccl::thread_scoped_lock pixels_lock(pixels_mutex);
		delete[] pixels;
		pixels = nullptr;
	}*/

private:
	bool _size_has_changed;

protected:
	/* Protected constructor, use CCSession::create to create a new CCSession. */
	CCSession(float* pixels_, unsigned int buffer_size_, unsigned int buffer_stride_)
		: 
			pixels{pixels_}, buffer_size{buffer_size_}, buffer_stride{buffer_stride_}
	{  }
};

class CCScene final {
public:
	/* Hold the Cycles scene. */
	ccl::Scene* scene = nullptr;

	unsigned int params_id = -1;

	/* Note: depth>1 if volumetric texture (i.e smoke volume data) */

	void builtin_image_info(const std::string& builtin_name, void* builtin_data, ccl::ImageMetaData& meta); // bool& is_float, int& width, int& height, int& depth, int& channels);
	bool builtin_image_pixels(const std::string& builtin_name, void* builtin_data, int tile, unsigned char* pixels, const size_t pixels_size, const bool associate_alpha, const bool free_cache);
	bool builtin_image_float_pixels(const std::string& builtin_name, void* builtin_data, int tile, float* pixels, const size_t pixels_size, const bool associate_alpha, const bool free_cache);
};

struct CCShader {
	/* Hold the Cycles shader. */
	ccl::Shader* shader = new ccl::Shader();
	/* Hold the shader node graph. */
	ccl::ShaderGraph* graph = new ccl::ShaderGraph();
	/* Map shader ID in scene to scene ID. */
	std::map<unsigned int, unsigned int> scene_mapping;
};

/* data */
extern std::vector<ccl::SceneParams*> scene_params;
extern std::vector<ccl::DeviceInfo> devices;
extern std::vector<ccl::DeviceInfo> multi_devices;
extern std::vector<ccl::SessionParams*> session_params;



/********************************/
/* Some utility functions       */
/********************************/

extern ccl::Shader* find_shader_in_scene(ccl::Scene* sce, unsigned int shader_id);
extern unsigned int get_idx_for_shader_in_scene(ccl::Scene* sce, ccl::Shader* sh);
extern bool scene_find(unsigned int scid, CCScene** csce, ccl::Scene** sce);
extern bool session_find(unsigned int sid, CCSession** ccsess, ccl::Session** session);
extern void scene_clear_pointer(ccl::Scene* sce);

extern void _cleanup_scenes();
extern void _cleanup_sessions();
extern void _cleanup_shaders();
extern void _init_shaders();

/********************************/
/* Some useful defines          */
/********************************/


/* Set boolean parameter varname of param_type. */
#define PARAM_BOOL(param_type, params_id, varname) \
	if (0 <= params_id && params_id < param_type.size()) { \
		param_type[params_id]-> varname = varname == 1; \
		logger.logit(client_id, "Set " #param_type " " #varname " to ", varname); \
	}

/* Set parameter varname of param_type. */
#define PARAM(param_type, params_id, varname) \
	if (0 <= params_id && params_id < param_type.size()) { \
		param_type[params_id]-> varname = varname; \
		logger.logit(client_id, "Set " #param_type " " #varname " to ", varname); \
	}

/* Set parameter varname of param_type, casting to typecast*/
#define PARAM_CAST(param_type, params_id, typecast, varname) \
	if (0 <= params_id && params_id < param_type.size()) { \
		param_type[params_id]-> varname = static_cast<typecast>(varname); \
		logger.logit(client_id, "Set " #param_type " " #varname " to ", varname, " casting to " #typecast); \
	}

#define LIGHT_FIND(scene_id, light_id) \
	CCScene* csce = nullptr; \
	ccl::Scene* sce = nullptr; \
	if(scene_find(scene_id, &csce, &sce)) { \
		ccl::Light* l = sce->lights[light_id]; \

#define LIGHT_FIND_END() \
		l->tag_update(sce); \
	}

#define SHADER_VAR2(a,b) a ## b
#define SHADER_VAR(a, b) SHADER_VAR2(a,b)
/* Set a var of shader to val of type. */
#define SHADER_SET(shid, type, var, val) \
	CCShader* sh = shaders[shid]; \
    sh->shader-> var = (type)(val); \
	logger.logit(client_id, "Set " #var " of shader ", shid, " to ", val, " casting to " #type);

