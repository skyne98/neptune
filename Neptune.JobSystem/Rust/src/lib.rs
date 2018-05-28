#![feature(ptr_internals)]

extern crate rayon;
extern crate cgmath;
extern crate libc;

use rayon::prelude::*;
use cgmath::SquareMatrix;

// C# interop wrappers
type ClrFuncOne<T0, T1> = extern "C" fn(T0) -> T1;
type ClrFuncTwo<T0, T1, T2> = extern "C" fn(T0, T1) -> T2;
type ClrFuncThree<T0, T1, T2, T3> = extern "C" fn(T0, T1, T2) -> T3;
type ClrFuncFour<T0, T1, T2, T3, T4> = extern "C" fn(T0, T1, T2, T3) -> T4;
type ClrFuncFive<T0, T1, T2, T3, T4, T5> = extern "C" fn(T0, T1, T2, T3, T4) -> T5;
type ClrFuncSix<T0, T1, T2, T3, T4, T5, T6> = extern "C" fn(T0, T1, T2, T3, T4, T5) -> T6;

type ClrActionOne<T0> = extern "C" fn(T0);
type ClrActionTwo<T0, T1> = extern "C" fn(T0, T1);
type ClrActionThree<T0, T1, T2> = extern "C" fn(T0, T1, T2);

type ClrGcHandle = libc::intptr_t;

// Rust to C# wrappers
fn to_pointer<T>(obj: T) -> *mut T {
    let boxed = Box::new(obj);
    let raw = Box::into_raw(boxed);

    return raw;
}

fn from_pointer<T>(obj_ptr: *mut T) -> T {
    unsafe {
        let boxed = Box::from_raw(obj_ptr);

        *boxed
    }
}

// Matrix
#[derive(Debug)]
pub struct Transform {
    x: f32,
    y: f32,
    rotation: f32,
    size_x: f32,
    size_y: f32,
    origin_x: f32,
    origin_y: f32,
}

#[derive(Debug)]
pub struct Matrix {
    m11: f32,
    m12: f32,
    m13: f32,
    m14: f32,
    m21: f32,
    m22: f32,
    m23: f32,
    m24: f32,
    m31: f32,
    m32: f32,
    m33: f32,
    m34: f32,
    m41: f32,
    m42: f32,
    m43: f32,
    m44: f32,
}

impl Matrix {
    fn from(&mut self, m: cgmath::Matrix4<f32>) {
        self.m11 = m.x[0];
        self.m12 = m.x[1];
        self.m13 = m.x[2];
        self.m14 = m.x[3];
        self.m21 = m.y[0];
        self.m22 = m.y[1];
        self.m23 = m.y[2];
        self.m24 = m.y[3];
        self.m31 = m.z[0];
        self.m32 = m.z[1];
        self.m33 = m.z[2];
        self.m34 = m.z[3];
        self.m41 = m.w[0];
        self.m42 = m.w[1];
        self.m43 = m.w[2];
        self.m44 = m.w[3];
    }
}

impl From<cgmath::Matrix4<f32>> for Matrix {
    fn from(m: cgmath::Matrix4<f32>) -> Self {
        Matrix {
            m11: m.x[0],
            m21: m.x[1],
            m31: m.x[2],
            m41: m.x[3],
            m12: m.y[0],
            m22: m.y[1],
            m32: m.y[2],
            m42: m.y[3],
            m13: m.z[0],
            m23: m.z[1],
            m33: m.z[2],
            m43: m.z[3],
            m14: m.w[0],
            m24: m.w[1],
            m34: m.w[2],
            m44: m.w[3],
        }
    }
}

#[no_mangle]
pub extern "C" fn calculate_matrices(transform_ptr: *mut Transform, transform_len: i32, matrix_ptr: *mut Matrix, matrix_len: i32) {
    let transforms = unsafe {
        assert!(!transform_ptr.is_null());

        std::slice::from_raw_parts(transform_ptr, transform_len as usize)
    };
    assert!(!matrix_ptr.is_null());

    let matrix_unique_ptr = std::ptr::Unique::new(matrix_ptr).unwrap();

    transforms.par_iter().enumerate().for_each(|(i, transform): (usize, &Transform)| {
        let matrices: &mut [Matrix] = unsafe {
            std::slice::from_raw_parts_mut(matrix_unique_ptr.as_ptr(), matrix_len as usize)
        };

        let mut result_matrix = cgmath::Matrix4::<f32>::from_translation(cgmath::Vector3 {
            x: transform.x,
            y: transform.y,
            z: 0.0,
        });
        result_matrix = result_matrix * cgmath::Matrix4::<f32>::from_angle_z(cgmath::Deg(transform.rotation));
        result_matrix = result_matrix * cgmath::Matrix4::<f32>::from_translation(cgmath::Vector3 {
            x: -transform.origin_x * transform.size_x,
            y: -transform.origin_y * transform.size_y,
            z: 0.0,
        });
        result_matrix = result_matrix * cgmath::Matrix4::<f32>::from_nonuniform_scale(transform.size_x, transform.size_y, 1.0);

        matrices[i].from(result_matrix);
    });
}